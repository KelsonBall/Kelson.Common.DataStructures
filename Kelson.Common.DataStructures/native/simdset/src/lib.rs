extern crate packed_simd;

pub trait ImmutableSet {
    fn count(&self) -> i32;
    fn shift(&self, delta : i32) -> Self;
    fn shift_adjacent(&self, delta : i32, adjacent : Self) -> Self;
    fn union(&self, other : Self) -> Self;
    fn intersection(&self, other : Self) -> Self;
    fn compliment(&self) -> Self;
    fn add(&self, value : i32) -> Self;
    fn add_vec(&self, values : Vec<i32>) -> Self;
    fn clear(&self) -> Self;
    fn contains(&self, value : i32) -> bool;
    fn except(&self, other : Self) -> Self;
    fn is_proper_subset_of(&self, other : Self) -> bool;
    fn is_subset_of(&self, other : Self) -> Self;
    fn is_proper_superset_of(&self, other : Self) -> bool;
    fn is_superset_of(&self, other : Self) -> bool;
    fn overlaps(&self, other : Self) -> bool;
    fn remove(&self, value : i32) -> Self;
    fn symmetric_except(&self, other : Self) -> Self;
}

impl ImmutableSet for u64 {
    fn count(&self) -> i32 { panic!("Not implemented") }
    fn shift(&self, delta : i32) -> Self { panic!("Not implemented") }
    fn shift_adjacent(&self, delta : i32, adjacent : Self) -> Self { panic!("Not implemented") }
    fn union(&self, other : Self) -> Self { *self | other }
    fn intersection(&self, other : Self) -> Self { *self & other }
    fn compliment(&self) -> Self { !*self }
    fn add(&self, value : i32) -> Self { panic!("Not implemented") }
    fn add_vec(&self, values : Vec<i32>) -> Self { panic!("Not implemented") }
    fn clear(&self) -> Self { 0 }
    fn contains(&self, value : i32) -> bool { panic!("Not implemented") }
    fn except(&self, other : Self) -> Self { *self & !other }
    fn is_proper_subset_of(&self, other : Self) -> bool { panic!("Not implemented") }
    fn is_subset_of(&self, other : Self) -> Self { panic!("Not implemented") }
    fn is_proper_superset_of(&self, other : Self) -> bool { panic!("Not implemented") }
    fn is_superset_of(&self, other : Self) -> bool { panic!("Not implemented") }
    fn overlaps(&self, other : Self) -> bool { (*self & other) != 0}
    fn remove(&self, value : i32) -> Self { panic!("Not implemented") }
    fn symmetric_except(&self, other : Self) -> Self { *self ^ other }
}

// impl ImmutableSet for packed_simd::u64x2 {

// }

// impl ImmutableSet for packed_simd::u64x4 {

// }

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn union() {
        let a : u64 = 1;
        let b : u64 = 2;

        let c = a.union(b);
        assert_eq!(c, 3)
    }
}
