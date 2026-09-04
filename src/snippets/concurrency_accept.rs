{
    // ...
    if updated {
        std::thread::spawn(move || {
            send_changed_event(&mut meeting, &target_user_id);
        });
    }
    accepted()
}