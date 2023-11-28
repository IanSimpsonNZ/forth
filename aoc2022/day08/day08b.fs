require ~/forth/libs/files.fs

128 Constant max-line
Create line-buffer  max-line 2 + allot

variable #rows
variable #cols
variable 'forrest

variable get-dist-height
variable get-dist-steps

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

: create-arrays     ( -- )
    create-forrest
    #cols ! #rows !
;

: process       ( -- )
    create-arrays
    cr
    'forrest .array
;

: get-dist      ( col row d-col d-row -- dist )
    1 get-dist-steps !
    2swap 2dup 'forrest rot rot at c@           ( dc dr c r h )
    get-dist-height !                           ( dc dr c r )
    begin
        2over rot + swap rot + swap             ( dc dr c' r' )
        2dup 0> swap 0>                         ( dc dr c' r' f f )
        2over #rows @ 1- < swap #cols @ 1- <    ( dc dr c' r' f f f f )
        and and and                             ( dc dr c' r' f )
    while                                       ( dc dr c' r' )
        2dup 'forrest rot rot at c@ dup         ( dc dr c' r' nh nh )
        get-dist-height @ dup                   ( dc dr c' r' nh nh h h )
        rot max get-dist-height !               ( dc dr c' r' nh h )
    < while                                     ( dc dr c' r' )
        1 get-dist-steps +!                     ( dc dr c' r' )
    repeat then                                 ( dc dr c r )
    2drop 2drop get-dist-steps @
;


: calc-score    ( col row -- score )
    2dup 0 -1 get-dist          ( col row s1 )
    rot rot                     ( s1 col row )
    2dup 0 1 get-dist           ( s1 col row s2 )
    2swap rot rot *             ( row col s )
    swap rot                    ( s col row )
    2dup -1 0 get-dist          ( s col row s3 )
    2swap rot rot *             ( row col s )
    swap rot                    ( s col row )
    2dup 1 0 get-dist           ( s col row s4 )
    2swap rot rot *             ( row col s )
    swap drop swap drop         ( s )
;

: .result       ( -- )
    0                           ( max )
    #rows @ 1- 1 do
        #cols @ 1-  1 do
            i j calc-score      ( max score )
            max                 ( max )
        loop
    loop
    cr ." The answer is: " . cr
;

: go
    open-input process .result close-input ;