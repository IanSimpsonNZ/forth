require ~/forth/libs/files.fs

128 Constant max-line
Create line-buffer  max-line 2 + allot

variable #rows
variable #cols
variable 'forrest
variable 'cansee

: at                ( 'array col row -- byte-addr)   \ Usage 'forrest 5 10 at @c
    #cols @ * + swap @ +              ( c+r*#c addr)
;

: .array            ( 'array -- )
    @
    #rows 0 do
        #cols 0 do
            dup i j * 
        loop
    loop
;
: .array            ( 'array -- )
    @                                   ( addr )
    #rows @ #cols @ * 0 do                  ( addr )
        i #cols @ mod 0= if cr then
        dup i + c@ .
    loop drop cr
;

: my-cmove          ( from to #chars -- )
    0 do                            ( from to )
        swap dup 1+ swap            ( to from+1 from)
        c@ [char] 0 -               ( to from+1 c-0 )
        rot dup 1+ rot rot c!       ( from+1 to+1 )
    loop 2drop
;

: test     ( -- file-name-addr file-name-len )
    s" test.txt" ;

: real     ( -- file-name-addr file-name-len )
    s" input.txt" ;

: get-num       ( num-chars -- n )
    0. rot line-buffer swap >number 2drop drop
;

: get-line      ( buff -- n-chr flag )
    line-buffer max-line fd-in read-line throw ;

: check-line-len    ( #cols len -- #cols )
    2dup <> if                  ( #c len )           \ If line len <> num columns
        swap dup 0<> if          ( len #c )           \ OK if this is the first line, set num cols
            -1 abort" Invalid line length" then        \ Otherwise mismatching line lengths
    then drop                   ( #c )
;

: create-forrest        ( -- #rows #cols )
    0 0                                     ( #rows #cols )
    here 'forrest !
    begin get-line while                    ( #r #c len )
        check-line-len                      ( #r #c )
        line-buffer over here swap          ( #r #c lb here #c )
        dup allot                           ( #r #c lb here #c )
        my-cmove                            ( #r #c )
        swap 1+ swap                        ( #r+1 #c )
    repeat drop                             ( #r #c )
;

: create-cansee         ( #r #c -- #r #c )
    here dup dup 'cansee !                  ( #r #c here here )
    2over * allot                           ( #r #c here here )
    2over * erase                           ( #r #c here )
    drop                                    ( #r #c )
;

: init-cansee           ( -- )              \ Expects #rows and #cols to be set
    #cols @ 0 do
        1 'cansee i 0 at c!
        1 'cansee i #rows @ 1- at c!
    loop

    #rows @ 1- 1 do
        1 'cansee 0 i at c!
        1 'cansee #cols @ 1- i at c!
    loop
;

: create-arrays     ( -- )
    create-forrest
    create-cansee
    #cols ! #rows !
    init-cansee
;

: visible?          ( max col row -- max )
    rot dup 2over                           ( col row max max col row )
    'forrest rot rot at c@ swap drop        ( col row max h )
    2dup < if                               ( col row max h )
        2swap 'cansee rot rot at 1 swap c!  ( max h )
    else 2swap 2drop then
    max
;

: set-visible  ( -- )
    #rows @ 1- 1 do
        'forrest 0 i at c@                      ( row-height-left )
        #cols @ 1- 1 do i j visible? loop       ( row-max )
        drop 'forrest #cols @ 1- i at c@        ( row-height-right )
        1 #cols @ 1- do i j visible? -1 +loop   ( row-max )
        drop
    loop

    #cols @ 1- 1 do
        'forrest i 0 at c@
        #rows @ 1- 1 do j i visible? loop
        drop 'forrest i #rows @ 1- at c@
        1 #rows @ 1- do j i visible? -1 +loop
        drop
    loop
;

: process       ( -- )
    create-arrays
    cr
    'forrest .array

    set-visible
    cr
    'cansee .array
;

: .result       ( -- )
    0
    'cansee @ dup #rows @ #cols @ * + swap do
        i c@ +
    loop
    cr ." The answer is: " . cr
;

: go
    open-input process .result close-input ;