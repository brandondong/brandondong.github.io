git() {
  local is_merge=false
  local is_commit=false
  case "$1" in
  "merge")
    is_merge=true
    ;;
  "commit")
    is_commit=true
    ;;
  *)
    # Not git merge/commit.
    command git "$@"
    return $?
    ;;
  esac

  local is_merge_abort=false
  local is_merge_continue=false
  if $is_merge; then
    for arg in "$@"; do
      case "${arg}" in
      "--quit")
        # git merge --quit -> leave state as is.
        command git "$@"
        return $?
        ;;
      "--abort")
        is_merge_abort=true
        break
        ;;
      "--continue")
        is_merge_continue=true
        break
        ;;
      esac
    done
  fi

  # Four cases: merge, merge abort, merge continue, commit.
  # For the latter three, double check that we're in the middle of a reverse merge before invoking the custom logic.
  # We maintain a marker file as a flag for whether the current merge was initiated from this alias.
  local reverse_merge_marker
  reverse_merge_marker=$(command git rev-parse --git-path reverse_merge_marker) || return $?
  if $is_commit || $is_merge_abort || $is_merge_continue; then
    # Check the marker file as well as sanity check MERGE_HEAD exists (created by git on merge).
    if [ ! -f "${reverse_merge_marker}" ] || ! command git rev-parse -q --verify MERGE_HEAD >/dev/null; then
      command git "$@"
      return $?
    fi
  fi

  local current_branch
  current_branch=$(command git branch --show-current)
  local is_detached_head=false
  [ -z "${current_branch}" ] && is_detached_head=true

  if $is_merge_abort; then
    # Merge abort case: perform the normal abort and then move back to the original location.
    # Due to reverse merge, this location will match what's in MERGE_HEAD.
    local original_SHA
    original_SHA=$(command git rev-parse -q --verify MERGE_HEAD)

    command git "$@" || return $?

    # Abort succeeded, remove merge marker and move back to original location.
    rm "${reverse_merge_marker}"
    if $is_detached_head; then
      command git checkout "${original_SHA}"
    else
      command git checkout -B "${current_branch}" "${original_SHA}"
    fi
    return $?
  fi

  local skip_merge_parent_swap=false
  if $is_merge && ! $is_merge_continue; then
    # git merge case:
    # First, parse the merge target.
    local target=""
    local -i i
    for ((i = 2; i <= $#; i++)); do
      local arg="${!i}"
      case "${arg}" in
      # Flags with values.
      "--cleanup" | "-s" | "--strategy" | "-X" | "--strategy-option" | "-m" | "--message" | "-F" | "--file" | "--into-name")
        i+=1
        ;;
      # Standalone flags.
      -*)
        ;;
      *)
        if [ -n "${target}" ]; then
          # Multiple targets (octopus), fall back to default merge.
          command git "$@"
          return $?
        fi
        target="${arg}"
        ;;
      esac
    done
    if [ -z "${target}" ]; then
      command git "$@"
      return $?
    fi

    # Try to move to the merge target.
    local current_SHA
    current_SHA=$(command git rev-parse HEAD)
    if $is_detached_head; then
      command git checkout --detach "${target}" || return $?
    else
      command git checkout -B "${current_branch}" "${target}" --no-track || return $?
    fi

    # Move succeeded, mark that the merge has started.
    touch "${reverse_merge_marker}"
    # Attempt reverse merge.
    local target_SHA
    target_SHA=$(command git rev-parse HEAD)
    # Generate a matching commit message to the normal direction.
    local merge_into_name
    if $is_detached_head; then
      merge_into_name="HEAD"
    else
      merge_into_name="${current_branch}"
    fi
    local target_type
    if [ "${target}" = "${target_SHA}" ]; then
      target_type="commit"
    else
      target_type="branch"
    fi
    local commit_message="Merge ${target_type} '${target}' into ${merge_into_name}"
    command git merge "${current_SHA}" -m "${commit_message}" || return $?

    # Merge completed successfully.
    # As there were no conflicts, check if it was a fast-forward merge in which case no merge parent swap is needed.
    if [ "${target_SHA}" != "$(command git rev-parse -q --verify HEAD^1)" ] ||
      [ "${current_SHA}" != "$(command git rev-parse -q --verify HEAD^2)" ]; then
      # Merge parents don't match expected merge from/into -> must be fast-forward merge.
      skip_merge_parent_swap=true
    fi
  else
    # git commit/merge --continue case: perform command as-is.
    command git "$@" || return $?

    # Sanity check we're no longer in the merging state (e.g. git commit --dry-run).
    if command git rev-parse -q --verify MERGE_HEAD >/dev/null; then
      return 0
    fi
  fi

  # Merge completed successfully.
  rm "${reverse_merge_marker}"
  if ! $skip_merge_parent_swap; then
    local existing_commit_message
    existing_commit_message=$(command git log -1 --format=%B)
    local swap_SHA
    swap_SHA=$(command git commit-tree -p HEAD^2 -p HEAD^1 -m "${existing_commit_message}" "HEAD^{tree}")

    if $is_detached_head; then
      command git checkout "${swap_SHA}" || return $?
    else
      command git checkout -B "${current_branch}" "${swap_SHA}" || return $?
    fi
  fi
}
