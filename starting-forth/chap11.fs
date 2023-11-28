
: star
    [char] * emit
;

: .row
    cr 8 0 do
        dup 128 and if star else space then
        1 lshift
    loop drop
;

: shape
    create 8 0 do c, loop
    does> dup 7 + do i c@ .row -1 +loop cr
;

hex
18 18 3c 5a 99 24 24 24 shape man
81 42 24 18 18 24 42 81 shape equis
aa aa fe fe 38 38 38 fe shape castle
decimal

