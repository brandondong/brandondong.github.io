 1 | fn process(mut meeting: Meeting) {
   |            ----------- move occurs because `meeting` has type `Meeting`, which does not implement the `Copy` trait
 2 |     std::thread::spawn(move || {
   |                        ------- value moved into closure here
 3 |         let result = do_work2();
 4 |         meeting.reload_and_retry_on_conflict(|meeting| {
   |         ------- variable moved due to use in closure
...
10 |     meeting.reload_and_retry_on_conflict(|meeting| {
   |     ^^^^^^^ value borrowed here after move