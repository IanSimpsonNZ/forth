: d@        ( addr -- d )           \ fetch double (surely this already exists?) - stores dl dh
    dup 1 cells + swap @ swap @
;

: d!        ( d addr -- )           \ store double (surely this already exists?)
    rot over ! 1 cells + !               (  )
;


( Random number generator -- High level)
variable rnd here rnd !
: random    rnd @ 31421 * 6927 + dup rnd ! ;
: choose    ( u1 -- u2 ) random um* nip ;

