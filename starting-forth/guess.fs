
: guess \ ( target guess -- target ) if target<>guess or (target guess -- ) if target = guess
   over - \ guess - target
   dup 0> if ." Too high" drop exit then
   0< if ." Too low" else drop ." Correct!" then
;

