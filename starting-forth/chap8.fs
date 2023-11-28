
0 constant reject
1 constant small
2 constant medium
3 constant large
4 constant extra-large
5 constant error

variable counts 5 cells allot
counts 6 cells erase

: counter cells counts + ;

: tally counter 1 swap +! ;

: category ( weight -- category)
    dup 18 < if reject      else
    dup 21 < if small       else
    dup 24 < if medium      else
    dup 27 < if large       else
    dup 30 < if extra-large else
    error
    then then then then then nip
;

: label ( category -- )
    case
        reject      of ." reject "      endof
        small       of ." small "       endof
        medium      of ." medium "      endof
        large       of ." large "       endof
        extra-large of ." extra-large " endof
        error       of ." error "       endof
    endcase
;

: eggsize category dup label tally ;

: report ( -- )
    page ." Quantity       Size " cr cr
    6 0 do i counter @ 5 u.r 7 spaces i label cr loop
;

variable pies
0 pies !

: eat-pie ( -- )
    pies @
    0> if -1 pies +! ." Thank you! "
        else ." What pie? "
    then
;

: bake-pie ( -- )
    1 pies +!
;

variable frozen-pies 0 ,

: freeze-pies ( -- )
    pies @ frozen-pies +!
    0 pies !
;

variable places 0 ,

: m. ( d -- )
    places @
    if
        <#
        places @ 0 do # loop [char] . hold
        #s #> type space
    else
        d.
    then
;

create hist-data 2 , 4 , 8 , 16 , 32 , 64 , 30 , 10 , 5 , 3 , 

: draw-line ( n -- )
    dup 0> if
        0 do [char] * emit loop
    else
        drop
    then
;

: plot ( -- )
    hist-data dup 10 cells + swap
    do cr i @ 5 u.r space i @ draw-line 1 cells +loop
    cr
;

