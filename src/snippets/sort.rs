fn sort(a: &mut [i32]) {
    for i in 0..a.len() {
        for j in 0..a.len() {
            if a[i] < a[j] {
                a.swap(i, j);
            }
        }
    }
}
