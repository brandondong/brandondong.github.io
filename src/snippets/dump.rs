/* fn process(mut meeting: Meeting) {
    std::thread::spawn(move || {
        let result = do_work2();
        meeting.reload_and_retry_on_conflict(|meeting| {
            // ... Update property Y based on result and save.
        });
    });

    let result = do_work1();
    meeting.reload_and_retry_on_conflict(|meeting| {
        // ... Update property X based on result and save.
    });
} */

pub fn admit_user(updated: bool, meeting: Meeting, target_user_id: String) {
    // ...
    if updated {
        std::thread::spawn(move || {
            send_changed_event(&meeting, &target_user_id);
        });
    }
    accepted()
}

pub struct Meeting;

impl Meeting {
    pub fn reload_and_retry_on_conflict<F, R>(&mut self, mut f: F) -> R
        where F: FnMut(&mut Meeting) -> R
    {
        f(self)
    }
}

pub fn do_work2() -> bool {true}
pub fn do_work1() -> bool {true}
fn accepted() {}
fn send_changed_event(_meeting: &Meeting, _s: &str) {}
