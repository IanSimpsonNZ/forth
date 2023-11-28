require ~/forth/libs/files.fs
require ~/forth/libs/queue.fs

128 Constant max-line
Create line-buffer  max-line 2 + allot

16384 constant move-high
16384 constant max-hash-len

variable 'hash


: test     ( -- file-name-addr file-name-len )
    s" test.txt" ;

: real     ( -- file-name-addr file-name-len )
    s" input.txt" ;

: get-num       ( num-chars -- n )
    0. rot line-buffer swap >number 2drop drop
;

: get-line      ( buff -- n-chr flag )
    line-buffer max-line fd-in read-line throw ;

: get-next-num       ( str len -- n addr rem )
    0. 2swap >number rot drop 1- swap 1+ swap ;  ( rot drop removes top of double result )

: move-head     ( hx hy dx dy -- hx' hy' )
    rot +           ( hx dx hy' )
    swap rot + swap ( hx' hy' )
;

: move-tail     ( tx ty hx hy -- tx' ty' hx hy )
    2swap 2over 2over           ( hx hy tx ty hx hy tx ty )
    rot swap -                  ( hx hy tx ty hx tx hy-ty)
    rot rot -                   ( hx hy tx ty hy-ty hx-tx )
    dup abs 1 > if              ( hx hy tx ty hy-ty hx-tx )     \ hx - tx > 1
        2 / 2swap rot rot +     ( hx hy hy-ty ty tx' )
        rot rot + 2swap         ( tx' ty' hx hy )
    else swap dup abs 1 > if    ( hx hy tx ty hx-tx hy-ty )      \ hy - ty > 1
        2 / rot +               ( hx hy tx hx-tx ty' )
        rot rot + swap 2swap    ( tx' ty' hx hy )
    else                        ( hx hy tx ty hx-tx hy-ty )
        2drop 2swap             ( tx ty hx hy )
    then then                   ( tx ty hx hy )
;

: search-key    ( key 'q-addr -- n|0 )
    0 rot rot                           ( 0 key 'addr )
    dup @ q-len dup if                  ( 0 key 'addr q-len)
        0 do                            ( 0 key 'addr )
            dup @ i q-data-ptr @        ( 0 key 'addr ith-n)
            2over swap drop = if        ( 0 key 'addr ith-n key )
                rot drop i rot rot      ( i key 'addr )
                leave
            then                        ( 0 key 'addr )
        loop
    else drop then                      ( 0 key 'addr )
    2drop
;

: store-tail    ( tx ty hx hy -- tx ty hx hy )
    2swap                               ( hx hy tx ty )
    2dup move-high * +                  ( hx hy tx ty key )
    dup 'hash search-key                ( hx hy tx ty key p|0 )
    if drop                             ( hx hy tx ty ) 
    else 'hash @ q-push then            ( hx hy tx ty )
    2swap                               ( tx ty hx hy )
;

: R 1 0 ;
: L -1 0 ;
: U 0 1 ;
: D 0 -1 ;

: move-rope     ( tx ty hx hy len  -- tx' ty' hx' hy' )
    2 - line-buffer 2 + swap get-next-num 2drop     ( tx ty hx hy moves )
    0 do                                            ( tx ty hx hy )
        line-buffer 1 evaluate                      ( tx ty hx hy dx dy )
        move-head                                   ( tx ty hx' hy' )
        move-tail                                   ( tx' ty' hx' hy' )
        store-tail                                  ( tx' ty' hx' hy' )
    loop
;

: process       ( -- )
    cr
    0 0 0 0                                 ( tx ty hx hy )
    begin get-line while                    ( tx ty hx hy len )
        dup 3 < if ." Invalid line: " line-buffer swap type cr
        else
            dup line-buffer swap type .s cr
            move-rope
        then
    repeat drop
    2drop 2drop
;

: setup         ( -- )
    max-hash-len 1 cells q-create 'hash !
;

: .result       ( -- )
    'hash @ q-len
    cr ." The answer is: " . cr
;

: go
    open-input setup process .result close-input ;