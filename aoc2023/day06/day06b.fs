require ~/forth/libs/files.fs
require ~/forth/libs/strings.fs

: test     ( -- file-name-addr file-name-len )
    s" test.txt" ;

: real     ( -- file-name-addr file-name-len )
    s" input.txt" ;


512 Constant max-line
Create line-buffer  max-line 2 + allot

512 $tring $time
512 $tring $distance


: get-line      ( -- num-chars flag ) line-buffer max-line fd-in read-line throw ;
: get-next-num       ( str len -- n addr rem )
    0. 2swap >number rot drop 1- swap 1+ swap ;  ( rot drop removes top of double result )


: get-value-string      ( addr len -- addr len )
    [char] : $split 2swap 2drop                             ( addr len )
    dup 2 < if -1 abort" No values after ':'" then           ( addr len )
    1- swap 1+ swap                                         ( addr+1 len-1 )        \ Move past ':'
;


: num-wins          ( time dist -- #wins )
    0 rot dup 1 > if                                        ( dist acc time )
        dup 1 do                                            ( dist acc time )
            dup i - i *                                     ( dist acc time this-dist )
            2over drop > if swap 1+ swap then               ( dist acc' time )
        loop drop swap drop                                 ( acc )
    else 0 then                                             ( #wins )
;

: calc-wins         ( d-addr d-len score t-val t-addr t-len -- d-addr' d-len' score' t-addr t-len )
    2swap 2rot                                              ( t-addr t-len score t-val d-addr d-len )
    $trim-front get-next-num                                ( t-addr t-len score t-val d-val d-addr' d-len' )
    rot dup 2rot 2swap drop                                 ( t-addr t-len d-addr' d-len' score t-val d-val )
    num-wins *                                              ( t-addr t-len d-addr' d-len' score )
    dup 2rot rot drop                                       ( d-addr' d-len' score t-addr t-len )
;

: process       ( -- n )
    cr
    get-line if line-buffer swap $time $copy else -1 abort" Could not read time line" then
    get-line if line-buffer swap $distance $copy else -1 abort" Could not read distance line" then
    $time type cr
    $distance type cr
    $distance get-value-string 1 $time get-value-string     ( d-addr d-len score t-addr t-len )
    begin $trim-front get-next-num dup 0> while             ( d-addr d-len score n t-addr t-len )
        calc-wins                                           ( d-addr d-len score t-addr t-len )
    repeat
    calc-wins                                               ( d-addr d-len score t-addr t-len )     \ Pick up last number pair
    2drop rot rot 2drop                                     ( score )
;

: .result       ( n -- )
    cr ." The answer is " . cr
;

: setup         ( -- )
;

: go
    open-input
    setup
    process 
    close-input
    .result
;
