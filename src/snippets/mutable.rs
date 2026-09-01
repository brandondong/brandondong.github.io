error[E0499]: cannot borrow `meeting` as mutable more than once at a time
 --> src/lib.rs:6:23
  |
2 |     let participants = meeting.get_all_participants();
  |                        ------- first mutable borrow occurs here
3 |     for participant in participants {
  |                        ------------ first borrow later used here
...
6 |         shared_helper(&mut meeting, &participant.id);
  |                       ^^^^^^^^^^^^ second mutable borrow occurs here