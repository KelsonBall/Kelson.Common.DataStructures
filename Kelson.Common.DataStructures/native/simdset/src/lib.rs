extern crate packed_simd;
extern crate generator;

use packed_simd::{ u32x4, u64x4 };

pub trait ImmutableSet where Self : std::marker::Sized {
    fn set_range_min() -> u32;
    fn set_range_max() -> u32;
    fn is_set_empty(&self) -> bool;
    fn is_set_full(&self) -> bool;
    fn new_empty_set() -> Self;
    fn new_full_set() -> Self;
    fn new(values : Vec<u32>) -> Self;
    fn count(&self) -> u32;
    fn shift_set(&self, delta : i32) -> Self;
    fn shift_adjacent(&self, delta : i32, adjacent : Self) -> Self;
    fn union(&self, other : Self) -> Self;
    fn intersection(&self, other : Self) -> Self;
    fn compliment(&self) -> Self;
    fn add(&self, value : u32) -> Self;
    fn add_vec(&self, values : Vec<u32>) -> Self;
    fn clear(&self) -> Self;
    fn contains(&self, value : u32) -> bool;
    fn except(&self, other : Self) -> Self;
    fn is_proper_subset_of(&self, other : Self) -> bool;
    fn is_subset_of(&self, other : Self) -> bool;
    fn is_proper_superset_of(&self, other : Self) -> bool;
    fn is_superset_of(&self, other : Self) -> bool;
    fn overlaps(&self, other : Self) -> bool;
    fn remove(&self, value : u32) -> Self;
    fn symmetric_except(&self, other : Self) -> Self;

    fn values(&self) -> ImmutableSetIterator<Self>;
}

pub struct ImmutableSetIterator<T : ImmutableSet + std::marker::Sized> {
    count : u32,
    value : T
}

impl Iterator for ImmutableSetIterator<u64> {
    type Item = u32;

    fn next(&mut self) -> std::option::Option<<Self as std::iter::Iterator>::Item> {
        while !self.value.is_set_empty() {                    
            let has_min = self.value.contains(u64::set_range_min());
            let value = self.count;
            self.value = self.value.shift_set(-1);
            self.count = self.count + 1;
            if has_min {
                return Option::Some(value)
            }
        }        
        return Option::None
    }
}

impl ImmutableSet for u64 {

    fn set_range_min() -> u32 { 0 }
    fn set_range_max() -> u32 { 63 }

    fn is_set_empty(&self) -> bool { *self == 0 }

    fn is_set_full(&self) -> bool { !*self == 0 }

    fn new_empty_set() -> u64 { 0u64 }

    fn new_full_set() -> u64 { !0u64 }

    fn new(values : Vec<u32>) -> u64 {
        let mut set = 0u64;
        for value in values {
            set |= 1u64 << value;
        }
        set
    }

    fn count(&self) -> u32 { (*self).count_ones() }

    fn shift_set(&self, delta : i32) -> Self {
        if delta > 0 {
            (*self) << delta
        }
        else if delta < 0 {
            (*self) >> -delta
        }
        else {
            panic!("No shift..")
            // *self
        }
    }

    fn shift_adjacent(&self, delta : i32, adjacent : Self) -> Self {
        if delta == 0 {
            return *self
        }

        let shifted = self.shift_set(delta);

        let adjacent = adjacent.shift_set(if delta < 0 { 64 } else { -64 } + delta);
        shifted.union(adjacent)
    }


    fn union(&self, other : Self) -> Self { *self | other }

    fn intersection(&self, other : Self) -> Self { *self & other }

    fn compliment(&self) -> Self { !*self }

    fn add(&self, value : u32) -> Self { *self | (1u64 << value) }

    fn add_vec(&self, values : Vec<u32>) -> Self {
        let mut set = *self;
        for value in values {
            set |= 1u64 << value;
        }
        set
    }

    fn clear(&self) -> Self { 0 }

    fn contains(&self, value : u32) -> bool {
        value <= u64::set_range_max() && *self & (1u64 << value) != 0
    }

    fn except(&self, other : Self) -> Self { *self & !other }

    fn is_proper_subset_of(&self, other : Self) -> bool {
        let values = *self;
        let dif = values ^ other; // elements that are not the same
        // this has no values different from other, and other has values different from this
        // TODO - is this a correct defining property of proper subsets and supersets?
        ((values & dif) == 0) && ((other & dif) != 0)
    }

    fn is_subset_of(&self, other : Self) -> bool {
        let values = *self;
        ((values & (values ^ other )) == 0)
    }

    fn is_proper_superset_of(&self, other : Self) -> bool {
        let values = *self;
        let dif = values ^ other; // elements that are not the same
        // this has values different from other, and other has no values different from this
        ((values & dif) != 0) && ((other & dif) == 0)
     }

    fn is_superset_of(&self, other : Self) -> bool {
        let values = *self;
        ((values & (values ^ other)) != 0)
     }

    fn overlaps(&self, other : Self) -> bool { (*self & other) != 0}

    fn remove(&self, value : u32) -> Self {
        *self & !(1u64 << value)
    }

    fn symmetric_except(&self, other : Self) -> Self { *self ^ other }

    fn values(&self) -> ImmutableSetIterator<u64> {
        ImmutableSetIterator { count: 0, value: *self }
    }
}

fn shift_lanes_left(v : &u64x4, delta : u32) -> u64x4 {
    if delta == 0 {
        *v
    }
    else if delta == 1 {
        u64x4::new(v.extract(1), v.extract(2), v.extract(3), 0u64)
    }
    else if delta == 2 {
        u64x4::new(v.extract(2), v.extract(3), 0u64, 0u64)
    }
    else if delta == 3 {
        u64x4::new(v.extract(3), 0u64, 0u64, 0u64)
    }
    else {
        u64x4::splat(0)
    }
}

fn shift_lanes_right(v : &u64x4, delta : u32) -> u64x4 {
    if delta == 0 {
        *v
    }
    else if delta == 1 {
        u64x4::new(0u64, v.extract(0), v.extract(1), v.extract(2))
    }
    else if delta == 2 {
        u64x4::new(0u64, 0u64, v.extract(0), v.extract(1))
    }
    else if delta == 3 {
        u64x4::new(0u64, 0u64, 0u64, v.extract(0))
    }
    else {
        u64x4::splat(0)
    }
}

impl ImmutableSet for u64x4 {

    fn set_range_min() -> u32 { 0 }
    fn set_range_max() -> u32 { 63 }

    fn is_set_empty(&self) -> bool { self.max_element() == 0u64 }

    fn is_set_full(&self) -> bool { self.min_element() == !0 }

    fn new_empty_set() -> u64x4 { u64x4::splat(0) }

    fn new_full_set() -> u64x4 { u64x4::splat(!0) }

    fn new(values : Vec<u32>) -> u64x4 {
        let mut sets = u64x4::splat(0);
        for value in values {
            let lane = (value >> 6) as usize;
            let set = sets.extract(lane);
            sets.replace(lane, set.add(value));
        }
        sets
    }

    fn count(&self) -> u32 { self.count_ones().wrapping_sum() as u32 }

    fn shift_set(&self, delta : i32) -> Self {
        // unimplemented!();

        if delta < 0 {
            let d = -delta as u32;
            let lane_delta = (d >> 6) as u32;
            let sub_delta = d % 64;

            let self_shifted = *self >> d;

        }
        

        unimplemented!()
    }

    fn shift_adjacent(&self, delta : i32, adjacent : Self) -> Self {
        unimplemented!()
        // if delta == 0 {
        //     return *self
        // }

        // let shifted = self.shift(delta);

        // let adjacent = adjacent.shift(if delta < 0 { 64 } else { -64 } + delta);
        // shifted.union(adjacent)
    }


    fn union(&self, other : Self) -> Self { *self | other }

    fn intersection(&self, other : Self) -> Self { *self & other }

    fn compliment(&self) -> Self { !*self }

    fn add(&self, value : u32) -> Self { unimplemented!() /* *self | (1u64x4 << value) */ }

    fn add_vec(&self, values : Vec<u32>) -> Self {
        unimplemented!()
        // let mut set = *self;
        // for value in values {
        //     set |= 1u64x4 << value;
        // }
        // set
    }

    fn clear(&self) -> Self { u64x4::new_empty_set() }

    fn contains(&self, value : u32) -> bool {
        self.extract((value >> 6) as usize).contains(value % 64)
    }

    fn except(&self, other : Self) -> Self { *self & !other }

    fn is_proper_subset_of(&self, other : Self) -> bool {
        unimplemented!();
        // let values = *self;
        // let dif = values ^ other; // elements that are not the same
        // // this has no values different from other, and other has values different from this
        // // TODO - is this a correct defining property of proper subsets and supersets?
        // ((values & dif) == 0) && ((other & dif) != 0)
    }

    fn is_subset_of(&self, other : Self) -> bool {
        unimplemented!()
        // let values = *self;
        // ((values & (values ^ other )) == 0)
    }

    fn is_proper_superset_of(&self, other : Self) -> bool {
        unimplemented!()
        // let values = *self;
        // let dif = values ^ other; // elements that are not the same
        // // this has values different from other, and other has no values different from this
        // ((values & dif) != 0) && ((other & dif) == 0)
     }

    fn is_superset_of(&self, other : Self) -> bool {
        unimplemented!()
        // let values = *self;
        // ((values & (values ^ other)) != 0)
     }

    fn overlaps(&self, other : Self) -> bool { unimplemented!() /* (*self & other) != 0 */ }

    fn remove(&self, value : u32) -> Self {
        unimplemented!()
        //*self & !(1u64x4 << value)
    }

    fn symmetric_except(&self, other : Self) -> Self { *self ^ other }

    fn values(&self) -> ImmutableSetIterator<u64x4> {
        ImmutableSetIterator { count: 0, value: *self }
    }
}

#[cfg(test)]
mod tests {
    use generator::{ done, Generator, Gn };
    use packed_simd::{ u8x2 };
use super::*;

    trait TestCases where Self : ImmutableSet + std::marker::Sized {

        fn small_test_set<'a>() -> Generator<'a, (), Self>;
        fn large_test_set<'a>() -> Generator<'a, (), Self>;

        fn count_matches_expected_size_cases<'a>() -> Generator<'a, (), (Self, u32)>;

        fn union_contains_all_items_cases<'a>() -> Generator<'a, (), (Self, Self, Self)>;
    }

    impl TestCases for u64 {

        fn small_test_set<'a>() -> Generator<'a, (), Self> {
            Gn::new_scoped(move |mut generator| {
                generator.yield_with(0);
                generator.yield_with(1);
                generator.yield_with(2);
                generator.yield_with(3);
                generator.yield_with(0b00000000_11111111_00000000_11111111_00000000_11111111_00000000_11111111);
                generator.yield_with(0b10000000_00000000_00000000_00000000_00000000_00000000_00000000_00000000);
                generator.yield_with(0b01010101_01010101_01010101_01010101_01010101_01010101_01010101_01010101);
                generator.yield_with(0b11111111_00000000_11111111_00000000_11111111_00000000_11111111_00000000);
                generator.yield_with(0b10101010_10101010_10101010_10101010_10101010_10101010_10101010_10101010);
                generator.yield_with(!0u64);
                done!();
            })
        }

        fn large_test_set<'a>() -> Generator<'a, (), Self> {
            Gn::new_scoped(move |mut generator| {
                for i in u64::set_range_min()..=u64::set_range_max() {
                    for round in u64::set_range_min()..u64::set_range_max() {
                        let mut set = 0u64;
                        for element in 0..i {
                            set = set.add((element + round) % (u64::set_range_max() + 1) + u64::set_range_min());
                        }
                        generator.yield_with(set);
                    }
                }
                generator.yield_with(u64::new_full_set());
                done!();
            })
        }

        fn count_matches_expected_size_cases<'a>() -> Generator<'a, (), (u64, u32)> {
            Gn::new_scoped(move |mut generator| {
                for i in u64::set_range_min()..=u64::set_range_max() {
                    for round in u64::set_range_min()..u64::set_range_max() {
                        let mut set = 0u64;
                        for element in 0..i {
                            set = set.add((element + round) % (u64::set_range_max() + 1) + u64::set_range_min());
                        }
                        generator.yield_with((set, i));
                    }
                }
                generator.yield_with((u64::new_full_set(), u64::set_range_max() - u64::set_range_min() + 1));
                done!();
            })
        }

        fn union_contains_all_items_cases<'a>() -> Generator<'a, (), (Self, Self, Self)> {
            Gn::new_scoped(move |mut generator| {
                generator.yield_with( (1, 2, 3) );
                generator.yield_with( (0, 10, 10) );
                generator.yield_with( (0, u64::new_full_set(), u64::new_full_set()) );
                generator.yield_with( (u64::new_full_set(), 0, u64::new_full_set()) );

                done!();
            })
        }
    }

    trait ImmutableSetTests where Self : ImmutableSet + std::marker::Sized {
        /// Count method of test set should be the same as the number of elements in the set
        /// For empty sets, expects 0
        /// For full sets, expects range_max + 1
        fn test_count_matches_expected_size(&self, expected_count : u32);

        /// Shift operation adjusts all elements in the set at once.
        /// Examples:
        /// An element that contains elements (1, 4, 9) will contain (2, 5, 10) after right shift 1
        /// An element that contains element 0 will not contain any elements after left shift 1
        fn test_shift_right_1_moves_all_items(&self);

        // fn test_shift_left_1_moves_all_items(params : Vec<(&Self, &Self)>);
        // fn test_shift_adjacent_includes_replacement_values(params : Vec<(&Self, &Self, &Self)>);

        fn test_union_contains_all_items(&self, other : Self, expected : Self);
    }

    impl ImmutableSetTests for u64 {

        fn test_count_matches_expected_size(&self, expected_count : u32) {
            assert_eq!(self.count(), expected_count)
        }

        fn test_union_contains_all_items(&self, other : u64, expected : u64) {
            assert_eq!(self.union(other), expected);
        }

        fn test_shift_right_1_moves_all_items(&self) {

            println!("original value: {}", *self);
            println!("original count: {}", self.count());
            let original_values : Vec<u32> = self.values().collect();
            println!("original: {:?}", original_values);

            let shifted = self.shift_set(1);      
            println!("shifted value: {}", shifted);
            let shifted_values : Vec<u32> = shifted.values().collect();
            println!("shifted: {:?}", shifted_values);

            // every value shifted up 1 should have its predecessor in the original set
            for value in shifted.values() {
                println!("{} in shifted -> {} should be in original", value, value - 1);
                assert_eq!(self.contains(value - 1), true);
            }

            // every value in the original set should have its successor in the shifted set, unless the value is at the end of the sets range
            for value in self.values() {
                if value + 1 < u64::set_range_max() {
                    println!("{} in original -> {} should be in shifted", value, value + 1);
                    assert_eq!(shifted.contains(value + 1), true);
                }
                else {
                    println!("{} in original -> should be dropped in shifted", value);
                }
            }

            let shifted_back = shifted.shift_set(-1);

            let shifted_back_values : Vec<u32> = shifted_back.values().collect();
            println!("shifted back: {:?}", shifted_back_values);

            if !self.contains(u64::set_range_max()) {
                // if no value was shifted out of range then the 'shifted_back' set should be equal to the original
                assert_eq!(*self, shifted_back);
            }
            else if self.count() > 0 {
                assert_eq!(shifted_back.is_subset_of(*self), true);                
            }
        }
    }

    #[test]
    fn test_count_matches_expected_size() {
        for (set, count) in u64::count_matches_expected_size_cases() {
            set.test_count_matches_expected_size(count);
        }
    }

    #[test]
    fn test_union_contains_all_items() {
        for (set, other, expected) in u64::union_contains_all_items_cases() {
            set.test_union_contains_all_items(other, expected);
        }
    }

    #[test]
    fn test_shift_right_1_moves_all_items() {
        for set in u64::small_test_set() {
            set.test_shift_right_1_moves_all_items();
        }
    }

    #[test]
    fn test_empty_is_always_subset() {
        for set in u64::small_test_set() {
            assert_eq!(0u64.is_subset_of(set), true);
        }
    }

    #[test]
    fn test_empty_is_proper_subset_of_non_empty_sets() {
        for set in u64::small_test_set() {
            assert_eq!(0u64.is_proper_subset_of(set), set != 0);
        }
    }

    #[test]
    fn verify_simd_shl_behavior() {
        let values = u8x2::new(0b1100_0000, 0b1000_0001);
        let next = values << 1;
        assert_eq!(next.extract(0), 0b1000_0000); // leading 1 of 2nd lane does not get shifted to trailing column of 1st lane
        assert_eq!(next.extract(1), 0b0000_0010);
    }
}


// impl ImmutableSet for packed_simd::u64x2 {

// }

// impl ImmutableSet for packed_simd::u64x4 {

// }
