
( Random number generator -- High level)
variable rnd here rnd !
: random    rnd @ 31421 * 6927 + dup rnd ! ;
: choose    ( u1 -- u2 ) random um* nip ;

