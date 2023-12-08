\ Approach
\ Need a three line buffer
\ Fill with all '.'
\ Get first line (line 0)
\ save line length
\ n = 0
\ do n=n+1 getline and line length ok while
\ get numbers from line n-1
\ repeat
\ get numbers from line n



require ~/forth/libs/files.fs

: test     ( -- file-name-addr file-name-len )
    s" test.txt" ;

: real     ( -- file-name-addr file-name-len )
    s" input.txt" ;

\ Create a three line buffer
512 Constant max-line
Create line-buffer  max-line 2 + 3 * allot

variable buff-len



variable max-red
variable max-green
variable max-blue

: 3dup          ( x a b c -- x a b c a b c )
    2over swap 2over rot drop 
;

: 4to1          ( a b c d -- d a b c )
    swap 2swap rot
;

: buff          ( n -- addr of line n )         3 mod max-line * line-buffer + ;
: blank         ( n -- )                        3 mod max-line * line-buffer + max-line [char] . fill ;
: get-line      ( buff-addr -- num-chars flag ) max-line fd-in read-line throw ;
: get-next-num       ( str len -- n addr rem )
    0. 2swap >number rot drop 1- swap 1+ swap ;  ( rot drop removes top of double result )

: char-num?     ( c -- f )
    dup [char] 0 >= swap [char] 9 <= and
;

: get-num-pos       ( line cpos -- epos n )
    swap buff over +                                    ( cpos c-addr )
    1                                                   ( cpos c-addr n-len )
    begin 2dup + c@ char-num? while 1+ repeat           ( cpos c-addr n-len' )
    swap over                                           ( cpos n-len' c-addr n-len' )
    get-next-num 2drop                                  ( cpos n-len' n )
    rot rot + swap                                      ( epos n )
;

: get-char          ( line cpos -- c )
    swap buff + c@
;


: is-num? ( line cpos -- f )
    get-char char-num?
;

: is-symbol?     ( line-num cpos -- f )
    dup dup 0< swap buff-len @ >= or if                 ( ln cpos )
        2drop false                                     ( f )
    else                                                ( ln cpos )
        get-char                                        ( c )
        dup [char] . =                                  ( c f )
        swap char-num?                                  ( f f )
        or invert                                       ( f )
    then                                                ( f )
;

: check-left    ( line-num cpos epos -- f )                  \ true if char left of number is a symbol
    drop                                                ( ln cpos )
    2dup 1- is-symbol? rot rot                          ( f1 ln cpos )
    2dup 1- swap 1- swap is-symbol? rot rot             ( f1 f2 ln cpos )
    1- swap 1+ swap is-symbol?                          ( f1 f2 f3 )
    or or                                               ( f )
;

: check-right    ( line-num cpos epos -- f )                  \ true if char left of number is a symbol
    swap drop                                           ( ln epos )         ( ln epos )
    2dup is-symbol? rot rot                             ( f1 ln epos )      ( ln epos f1)
    2dup swap 1- swap is-symbol? rot rot                ( f1 f2 ln epos )
    swap 1+ swap is-symbol?                             ( f1 f2 f3 )
    or or                                               ( f )
;


: check-top-bot     ( line-num cpos epos -- f )
    swap 2dup >= if                                  ( ln epos-1 cpos )
        false rot rot                                   ( ln f epos-1 cpos )
        do                                              ( ln f )
            over i is-symbol? or                        ( ln f )
            dup if leave then                           ( ln f )
        loop swap drop                                  ( f )
    else 2drop drop false then                          ( f )
;


: is-part-num?  ( line-num cpos epos -- f )
    0 4to1                                              ( 0 ln cpos epos )              \ Prepare for 3dups (ie need 4 elements on stack)
    3dup check-left 4to1                                ( 0 f1 ln cpos epos )
    3dup check-right 4to1                               ( 0 f1 f2 ln cpos epos )
    3dup rot 1- rot rot check-top-bot 4to1              ( 0 f1 f2 f3 ln cpos epos )
    rot 1+ rot rot check-top-bot                        ( 0 f1 f2 f3 f4 )
    or or or swap drop                                  ( f )
;

: check-num     ( line-num cpos -- incr n )                                            \ check for a number and return position after number
    2dup is-num? if                                     ( ln cpos )
        2dup get-num-pos                                ( ln cpos epos n )
        swap 2swap rot                                  ( n ln cpos epos )
        3dup is-part-num? if                            ( n ln cpos epos )
            swap - swap drop swap                       ( len n )
        else
            swap - swap drop swap drop 0                ( len 0 )
        then
    else 2drop 1 0 then
;


: process-line  ( line-num -- line-tot )
    dup buff buff-len @ type ."    "
    0                                                   ( line-num acc )
    buff-len @ 0 do                                     ( line-num acc )
        over i check-num                                ( line-num acc loop-incr num )
        dup dup 0> if . ."    " else drop then
        rot + swap                                      ( line-num acc loop-incr ) 
    +loop                                               ( line-num acc )
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