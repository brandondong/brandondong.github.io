fn lower_hands(mut meeting: Meeting) {
    let participants = meeting.get_all_participants();
    for participant in participants {
        participant.set_is_hand_raised(false);

        shared_helper(&mut meeting, &participant.id);
    }

    meeting.save();
}

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

pub fn admit_user(updated: bool, mut meeting: Meeting, target_user_id: String) {
    // ...
    if updated {
        std::thread::spawn(move || {
            send_changed_event(&mut meeting, &target_user_id);
        });
    }
    accepted()
}

pub struct Meeting {
    pub v: Vec<Participant>,
}

impl Meeting {
    pub fn reload_and_retry_on_conflict<F, R>(&mut self, mut f: F) -> R
        where F: FnMut(&mut Meeting) -> R
    {
        f(self)
    }

    pub fn get_all_participants(&mut self) -> &mut [Participant] {
        &mut self.v
    }

    pub fn save(&mut self) {}
}

pub fn do_work2() -> bool {true}
pub fn do_work1() -> bool {true}
fn accepted() {}
fn send_changed_event(_meeting: &mut Meeting, _s: &str) {}
pub fn shared_helper(_meeting: &mut Meeting, _s: &str) {}
pub struct Participant {
    id: String,
}

impl Participant {
    fn set_is_hand_raised(&mut self, _b: bool) {}
}
