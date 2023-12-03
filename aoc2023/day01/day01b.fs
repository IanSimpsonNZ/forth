require ~/forth/libs/files.fs

: test     ( -- file-name-addr file-name-len )
    s" test.txt" ;

: real     ( -- file-name-addr file-name-len )
    s" input.txt" ;

create one 3 allot s" one" one swap move
create two 3 allot s" two" two swap move
create three 5 allot s" three" three swap move
create four 4 allot s" four" four swap move
create five 4 allot s" five" five swap move
create six 3 allot s" six" six swap move
create seven 5 allot s" seven" seven swap move
create eight 5 allot s" eight" eight swap move
create nine 4 allot s" nine" nine swap move


512 Constant max-line
Create line-buffer  max-line 2 + allot

: get-line      ( -- num-chars flag )   line-buffer max-line fd-in read-line throw ;
: get-num       ( num-chars -- n )      0. rot line-buffer swap >number 2drop drop ;

: ischar?       ( n -- f)
    dup 0 >= swap 9 <= and
;

: get-next-digit    ( len pos -- n pos f )          \ start at pos in input-buffer and find next char between 0-9
    2dup > if                                   ( len pos )     \ trying to search past end of line?
        -1 over                                 ( len pos n i ) \ defaults -> digit -1, pos = initial pos
        2swap do                                ( n i )
            line-buffer i + c@ [char] 0 -       ( n i n? )
            dup ischar? if                      ( n i n' )
                rot drop swap drop i            ( n' i' )
                leave
            then drop
        loop
        over -1 = if                            ( n i )   \ Failed to find a digit,
            0                                   ( -1 pos0 0 ) \ flag = false
        else
            1+ -1                               ( n i+1 -1 )  \ flag = true, pos set to i+1 for next search
        then
    else 0 then                                 ( len pos 0 ) \ trying to search past end of line so fail
;

: comparing             ( 'start len 'sub sub-len -- 'start len 'sub sub-len )
    2over 2over
    .s
    ."  Looking for '" type ." ' in '" type ." '" cr
;

: same?                 ( 'start len 'sub sub-len -- t|f )
    rot 2dup > if                   ( 'start 'sub sub-len len )     \ do we have enough chars for the number?
        2drop 2drop 0               ( 0 )                           \ no, so we can't match
    else
        drop                            ( 'start 'sub len )
        -1 swap 2swap rot               ( -1 'start 'sub len )               \ result - default true
        0 do                            ( f 'start 'sub )
            2dup                        ( f 'start 'sub 'start 'sub )
            i + c@ swap                 ( f 'start 'sub sub-c 'start' )
            i + c@ <> if                ( f 'start 'sub )
                2drop drop 0 0 0
                leave
            then                        ( f 'start 'sub )
        loop                            ( f 'start 'sub )
        2drop
    then 
;

: check-nums            ( len start-pos -- n f )
\    .s
    dup line-buffer +                           ( len start-pos 'start )
    rot rot -                                   ( 'start len )
\    ." line-buffer start: " line-buffer . .s cr
    2dup one 3 same? if 1 -1
    else 2dup two 3 same? if 2 -1
    else 2dup three 5 same? if 3 -1
    else 2dup four 4 same? if 4 -1
    else 2dup five 4 same? if 5 -1
    else 2dup six 3 same? if 6 -1
    else 2dup seven 5 same? if 7 -1
    else 2dup eight 5 same? if 8 -1
    else 2dup nine 4 same? if 9 -1
    else 0 0
    then then then then then then then then then    ( 'start len n f )
    2swap 2drop                                     ( n f )
;

: get-next-spelled    ( len pos -- n pos f )          \ start at pos in input-buffer and find next spelled digit
    over swap                                   ( len len pos)
    2dup + 2 > if                               ( len len pos )     \ trying to search past end of line (incl 3 chars for shortest word ie "one") ?
        -1 over                                 ( len len pos n i ) \ defaults -> digit -1, pos = initial pos
        2swap do                                ( len n i )
            rot dup i check-nums if             ( n i len n' )
                2swap 2drop i                   ( len n' i' )
                leave
            else drop rot rot then              ( len n i)
        loop
        rot drop                                ( n i )
        over -1 = if                            ( n i )   \ Failed to find a digit,
            0                                   ( -1 pos0 0 ) \ flag = false
        else
            1+ -1                               ( n i+1 -1 )  \ flag = true, pos set to i+1 for next search
        then
    else rot drop 0 then                        ( len pos 0 )    
;


: get-first     ( num-chars -- n pos+1 f )
    dup 0 get-next-spelled                      ( len n-s pos-s f-s)
    2swap swap                                  ( pos-s f-s n-s len )

    0 get-next-digit                            ( pos-s f-s n-s n-d pos-d f-d )

    2rot rot                                    ( n-s n-d pos-d pos-s f-s f-d )
    2dup and if                                 ( n-s n-d pos-d pos-s f-s f-d )     \ found digit and string, get earliest one
        2drop                                   ( n-s n-d pos-d pos-s )
        2dup < if                               ( n-s n-d pos-d pos-s )
            drop rot drop -1                 ( n-d pos-d+1 -1 )
        else
            swap drop swap drop -1           ( n-s pos-s+1 -1 )
        then
    else                                        ( n-s n-d pos-d pos-s f-s f-d )
        if                                      ( n-s n-d pos-d pos-s f-s )         \ found digit, but not string
            2drop rot drop -1                ( n-d pos-d+1 -1 )
        else                                    ( n-s n-d pos-d pos-s f-s )
            if                                  ( n-s n-d pos-d pos-s )             \ found string, not digit
                swap drop swap drop -1       ( n-s pos-s+1 -1 )
            else                                ( n-s n-d pos-d pos-s )             \ found neither
                2drop 2drop 0 0 0               ( 0 0 0 )
            then
        then
    then
;

: get-last-digit      ( num-chars start-pos -- n pos+1 f ) \ need to allow same digit being first and last ...
    1-                                              \ get-first will have incremented pos, so can safely 1-
 \   2dup ."    Searching from " . ."  to  " . cr
    swap over 0 rot rot                             ( s0 0 len s) \ defaults - n 0
    begin 2dup get-next-digit while                 ( s0 n len s n' s' )
        2swap drop 2swap swap drop swap rot         ( s0 n' len s' )
    repeat 2drop                                    ( s0 n len s )
    swap drop rot over = if                         ( n s )   \ if didn't find a digit, s0 = s
        0                                           ( n s 0 ) \ set flag to false
    else
        -1                                          ( n s -1 ) \ found digit and s is pos of char after digit
    then
;

: get-last-spelled      ( num-chars start-pos -- n pos+1 f )
    1-                                              ( len s-1 )         \ get-first will have incremented pos, so can safely 1-
    over 2 -                                        ( len s-1 len-3 )       \ need at least 3 chars
    2dup > if
        2drop drop 0 0 0
    else
        -1 0 2swap                                      ( len n f s-1 len-3 )  \ set defaults for n (-1) and flag (false)
        do                                              ( len n f )
            rot dup                                     ( n f len len )
            i check-nums if                             ( n f len n' )
                2swap 2drop swap drop i 1+ -1           ( n' pos+1 true )
                leave
            else                                        ( n f len n' )
                drop rot rot                            ( len n f )
            then
        -1 +loop
    then
;

: get-last      ( num-chars start-pos -- n pos+1 f )
    2dup get-last-spelled                       ( len start n-s pos-s f-s)
    0 2rot                                      ( n-s pos-s f-s 0 len start )

    get-last-digit                              ( n-s pos-s f-s 0 n-d pos-d f-d )

    2rot rot                                    ( n-s 0 n-d pos-d pos-s f-s f-d )
    2dup and if                                 ( n-s 0 n-d pos-d pos-s f-s f-d )     \ found digit and string, get earliest one
        2drop                                   ( n-s 0 n-d pos-d pos-s )
        2dup > if                               ( n-s 0 n-d pos-d pos-s )
            drop rot drop rot drop -1           ( n-d pos-d -1 )                       \ digit is last
        else
            swap drop swap drop swap drop -1    ( n-s pos-s -1 )                        \ string is last
        then
    else                                        ( n-s 0 n-d pos-d pos-s f-s f-d )
        if                                      ( n-s 0 n-d pos-d pos-s f-s )         \ found digit, but not string
            2drop rot drop rot drop -1          ( n-d pos-d -1 )
        else                                    ( n-s 0 n-d pos-d pos-s f-s )
            if                                  ( n-s 0 n-d pos-d pos-s )             \ found string, not digit
                swap drop swap drop swap drop -1    ( n-s pos-s -1 )
            else                                ( n-s 0 n-d pos-d pos-s )             \ found neither
                2drop 2drop drop 0 0 0          ( 0 0 0 )
            then
        then
    then
;

: process-line  ( num-chars -- n )
    line-buffer over type
    dup 2 < if -1 abort" Line too short" then                                               ( len )
    dup get-first if swap ( dup . ) 10 * swap else -1 abort" Can't find first number" then      ( len 10n pos+1 )
    rot swap get-last if drop ( dup . ) + else -1 abort" Can't find last number" then           ( 10n+n )
    cr
;

: process       ( -- n )
    0                               \ accumulator
    cr begin get-line while
        process-line +
    repeat drop
;

: .result       ( n -- )
    cr ." The total is " . cr
;

: setup         ( -- )
;

: go            ( filename len -- )
    open-input
    setup
    process 
    close-input
    .result
;