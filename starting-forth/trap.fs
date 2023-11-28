
: within  ( n lo hi+1 -- flag )   over - rot rot - u> ;
: 3dup ( a b c -- a b c a b c ) dup 2over rot ;
: trap ( target guess-low guess-high -- target | )
    3dup over = rot rot = and if ." You got it!" 2drop drop else
        3dup swap 1+ swap within if ." Between" else
        ." Not between"
    then 2drop then
;