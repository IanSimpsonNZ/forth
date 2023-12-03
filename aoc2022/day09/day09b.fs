require ~/forth/libs/files.fs
require ~/forth/libs/queue.fs

128 Constant max-line
Create line-buffer  max-line 2 + allot

0
1 cells field x
1 cells field y
constant #coord

: rope  create #coord * allot                   \ Usage '15 rope my-rope'
    does> swap #coord * + ;                     \ Usage 'n my-rope' return address of nth xy coord pair 

10 constant rope-length
rope-length rope my-rope

16 constant move-high
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

: get-knot      ( addr -- x y )           \ Usage ' 5 my-rope get-knot'
    dup x @ swap y @
;

: move-head     ( dx dy -- )
    0 my-rope get-knot      ( dx dy hx hy )
    rot +                   ( dx hx hy' )
    0 my-rope y !           ( dx hx )
    + 0 my-rope x !         (  )
;

: move-tail2    ( tx ty hx hy -- tx' ty' )
    2over                       ( tx ty hx hy tx ty )
    rot swap -                  ( tx ty hx tx hy-ty )
    rot rot -                   ( tx ty hy-ty hx-tx )
    dup abs 1 > if              ( tx ty hy-ty hx-tx )     \ hx - tx > 1
        2 / 2swap rot rot +     ( hy-ty ty tx' )
        rot                     ( ty tx' hy-ty )
        dup abs 1 > if 2 / then  ( ty tx' hy-ty )
        rot +                   ( tx' ty' )
    else swap dup abs 1 > if    ( tx ty hx-tx hy-ty )      \ hy - ty > 1        
        2 / rot +               ( tx hx-tx ty' )
        swap                    ( tx ty' hx-tx )
        dup abs 1 > if 2 / then  ( tx ty' hx-tx )
        rot + swap              ( tx' ty' )
    else                        ( tx ty hx-tx hy-ty ) 
        2drop                   ( tx ty )
    then then                   ( tx ty )
;

: move-rope     ( -- )
    rope-length 1 do
        i my-rope get-knot      ( tx ty )
        i 1- my-rope get-knot   ( tx ty hx hy )
        move-tail2              ( tx' ty' )
        i my-rope y !           ( tx' )
        i my-rope x !           (  )
    loop
;

: search-key    ( key 'q-addr -- n+1|0 )
    0 rot rot                           ( 0 key 'addr )
    dup @ q-len dup if                  ( 0 key 'addr q-len)
        0 do                            ( 0 key 'addr )
            dup @ i q-data-ptr @        ( 0 key 'addr ith-n)
            2over swap drop = if        ( 0 key 'addr ith-n key )
                rot drop i 1+ rot rot   ( i key 'addr )                 \ so we never return position 0
                leave
            then                        ( 0 key 'addr )
        loop
    else drop then                      ( 0 key 'addr )
    2drop
;

: store-tail    ( -- )
    rope-length 1- my-rope get-knot     ( tx ty )
    move-high lshift +                       ( key )
    dup 'hash search-key                ( key p|0 )
    if drop                             (  ) 
    else 'hash @ q-push then            (  )
;

: R 1 0 ;
: L -1 0 ;
: U 0 1 ;
: D 0 -1 ;

: move-rope     ( len -- )                                                         \ old ( tx ty hx hy len  -- tx' ty' hx' hy' )
    2 - line-buffer 2 + swap get-next-num 2drop     ( moves )
    0 do                                            ( )
        line-buffer 1 evaluate                      ( dx dy )
        move-head                                   ( )
        move-rope                                   ( )
        store-tail                                  ( )
    loop
;

: process       ( -- )
    cr
    begin get-line while                    ( len )
        dup 3 < if ." Invalid line: " line-buffer swap type cr
        else
\            dup line-buffer swap type .s cr
            move-rope
        then
    repeat drop
;

: setup         ( -- )
    max-hash-len 1 cells q-create 'hash !
    0 my-rope rope-length #coord * erase
;

: .result       ( -- )
    'hash @ q-len
    cr ." The answer is: " . cr
;

: go
    open-input setup process .result close-input ;