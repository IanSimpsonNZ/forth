
: star [char] * emit ;

: stars ( n -- ) dup 0> if 0 do star loop then ;

: box ( width height -- )
    dup 0> if 
        swap dup 0> if
            swap 0 do cr dup stars loop drop
        else 2drop then
    else 2drop then
;

: \stars ( height -- )
    dup 0> if
        0 do
            cr i spaces 10 stars loop
    else drop then
;

\ : /stars ( height -- )
\     1- dup 0> if
\        0 swap do
\            cr i spaces 10 stars -1 +loop
\    else drop then
\ ; 

 : /stars ( height -- )
     dup 0> if
        begin
            cr 1- dup spaces 10 stars
        dup 0= until
    else drop then
;

: triangle ( increment limit start -- )
    do cr 9 i - spaces i 2* 1+ stars dup +loop drop
;

: diamond ( -- )
    1 10 0 triangle
    -1 0 9 triangle
;

: diamonds ( n -- )
    0 do diamond loop quit
;

: r% 10 */ 5 + 10 / ;

 
: doubled ( start-bal int -- )
    over            \ 1000 6 1000
    21 1 do cr ." Year " i 2 u.r
        2dup r% + dup ."   Balance " . .s     \ 1000 6 1060
        rot 2dup / 2 = if cr ." More than doubled in " i . ." years " leave  \ 6 1060 1000
         else rot rot then 
    loop 2drop drop
;

: ** ( x y -- x^y )
    over swap \ x x y
    2 swap do over * -1 +loop
    nip
;