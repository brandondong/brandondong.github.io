{
    // ...
    if updated {
        std::thread::spawn(move || {
            send_changed_event(&meeting, &target_user_id);
        });
    }
    accepted()
}