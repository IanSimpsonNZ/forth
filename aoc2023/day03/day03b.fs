require ~/forth/libs/files.fs

: test     ( -- file-name-addr file-name-len )
    s" test.txt" ;

: real     ( -- file-name-addr file-name-len )
    s" input.txt" ;

\ Create a three line buffer
512 Constant max-line
Create line-buffer  max-line 2 + 3 * allot

variable buff-len

: buff          ( n -- addr of line n )         3 mod max-line * line-buffer + ;
: blank         ( n -- )                        3 mod max-line * line-buffer + max-line [char] . fill ;
: get-line      ( buff-addr -- num-chars flag ) max-line fd-in read-line throw ;
: get-next-num       ( str len -- n addr rem )
    0. 2swap >number rot drop 1- swap 1+ swap ;  ( rot drop removes top of double result )

: char-num?     ( c -- f )
    dup [char] 0 >= swap [char] 9 <= and
;

: get-char          ( line cpos -- c )
    swap buff + c@
;

: is-num? ( line cpos -- f )
    get-char char-num?
;

: in-range?      ( cpos -- f )
    dup 0>= swap buff-len @ < or
;


: is-star?        ( line-num cpos -- f )
    swap buff + c@ [char] * =
;

: bound-isnum?      ( line-num cpos -- f )
    dup in-range? if is-num?                            ( f )
    else 2drop false then
;

: check-line        ( line-num cpos -- n )              \ count number of numbers in the three characters above a * 0, 1 or 2
                                                        \ line-num has already been set to one line above or below
    2dup bound-isnum? if                                ( ln cpos)
        2drop 1                                         ( 1 )                   \ if num in middle then only one number
    else                                                ( ln cpos )             \ check corners
        0 0                                             ( ln cpos 0 acc )
        2over 1- bound-isnum? if 1+ then                ( ln cpos 0 acc' )
        2swap 1+ bound-isnum? if 1+ then                ( 0 acc'' )
        swap drop                                       ( acc )
    then
;


: count-numbers     ( line-num cpos -- n )
    0 0                                                 ( ln cpos 0 acc )
    2over 1- bound-isnum? if 1+ then                    ( ln cpos 0 acc )       \ left
    2over 1+ bound-isnum? if 1+ then                    ( ln cpos 0 acc )       \ right
    2over swap 1- swap check-line +                     ( ln cpos 0 acc' )      \ above
    2swap swap 1+ swap check-line +                     ( 0 acc'' )             \ below
    swap drop
;

: extract-num       ( line-num cpos -- n )              \ assumes char has been confirmed as a digit
    2dup                                                ( ln cpos ln start-pos )
    begin 2dup bound-isnum? while 1- repeat 1+          ( ln cpos ln start-pos )
    swap drop rot rot                                   ( start-pos ln cpos )
    begin 2dup bound-isnum? while 1+ repeat             ( start-pos ln end-pos )
    swap buff rot dup rot + rot rot -                   ( s-addr len )
    get-next-num 2drop                                  ( n )
;

: nums-from-line    ( line-num cpos -- n )              \ gives single number (if middle) or left and right multiplied
    0 1 2swap                                                       ( 0 acc ln cpos )
    2dup bound-isnum? if                                            ( 0 acc ln cpos )               \ middle
        extract-num *                                               ( 0 acc )
    else                                                            ( 0 acc ln cpos )
        2dup 1- bound-isnum? if                                     ( 0 acc ln cpos )               \ left
            2dup 1- extract-num                                     ( 0 acc ln cpos n )
            2swap swap rot * swap rot                               ( 0 acc ln cpos )
        then                                                        ( 0 acc ln cpos )

        1+ 2dup bound-isnum? if                                     ( 0 acc ln cpos+1 )             \ right
            extract-num *                                           ( 0 acc )
        else 2drop then                                             ( 0 acc )
    then
    swap drop                                                       ( acc )
;

: calc-gears        ( line-num cpos -- n )
    2dup count-numbers                                                  ( ln cpos n )
    2 = if                                                              ( ln cpos )
        0 1                                                             ( ln cpos 0 acc )
        2over 1- 2dup bound-isnum? if extract-num * else 2drop then     ( ln cpos 0 acc )               \ left
        2over 1+ 2dup bound-isnum? if extract-num * else 2drop then     ( ln cpos 0 acc )               \ right
        2over swap 1- swap nums-from-line *                             ( ln cpos 0 acc )               \ top
        2swap swap 1+ swap nums-from-line *                             ( 0 acc )                       \ bottom
        swap drop                                                       ( acc )
    else                                                                ( ln cpos )
        2drop 0                                                         ( 0 )
    then
;


: process-line  ( line-num -- line-tot )
    dup buff buff-len @ type ."    "
    0                                                   ( line-num acc )
    buff-len @ 0 do                                     ( line-num acc )
        over i is-star? if                              ( line-num acc )
            over i calc-gears +                         ( line-num acc)
        then                                            ( line-num acc)
    loop                                                ( line-num acc )
    swap drop                                           ( acc )
    cr
;

: process       ( -- n )
    0 0                                                 ( acc n )           \ accumulator and line-number
    cr                                                  ( acc n )
    dup buff get-line if                                ( acc n len )
        buff-len !                                      ( acc n )
        begin
            1+                                          ( acc n+1 )
            dup buff get-line                           ( acc n+1 len f )   \ File line stored in buffer line n mod 3
            swap buff-len @ = and                       ( acc n+1 f )       \ check we read line and it was right length
        while                                           ( acc n+1 )
            dup 1- process-line                         ( acc n+1 line-tot ) \ process previous line
            rot + swap                                  ( acc n+1 )
        repeat                                          ( acc n+1 )
        dup blank                                       ( acc n+1 )         \ clear the line after eof
        1- process-line +                               ( acc )             \ process the last line
    else drop -1 abort" Can't read first line." then
;

: .result       ( n -- )
    cr ." The total is " . cr
;

: setup         ( -- )
    0 blank 1 blank 2 blank
;

: go
    open-input
    setup
    process 
    close-input
    .result
;