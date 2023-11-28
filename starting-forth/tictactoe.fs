decimal
create board 9 allot
: clear board 9 erase ; clear
: get-cell ( board-pos -- cell-value)
    1- board + c@
;


: mark. ( board-pos -- )
    get-cell
    dup 0= if ."   " else \ 
    dup 1 = if ." X " else \ 
    ." O "
    then then drop
;

: line. ( -- )
    9 0 do [char] - emit loop
;

: board. ( -- )
    cr
    10 1 do
        i mark.
        i 3 mod 0= if
                        cr
                        i 7 < if line. cr then
                    else ." | " then

    loop cr
;

: set-cell ( pos xory -- )
    swap
    dup 1 < swap dup 9 > rot or if ." Invalid position, must be 1 to 9" 2drop quit then
    dup get-cell
    0= if 1- board + c! else ." Cell already taken" 2drop then
;

: x! ( board-pos -- )
    1 set-cell board.
;

: o! ( board-pos --)
    -1 set-cell board.
;

