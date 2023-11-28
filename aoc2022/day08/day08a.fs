require ~/forth/libs/files.fs

128 Constant max-line
Create line-buffer  max-line 2 + allot

variable #rows
variable #cols
variable 'forrest
variable 'cansee

: at                ( 'array col row -- byte-addr)   \ Usage 'forrest 5 10 at @c
    #cols * + swap @ +
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

: create-arrays     ( -- )
    0 0                                     ( #rows #cols )
    here 'forrest !
    begin get-line while                    ( #r #c len )
        check-line-len                      ( #r #c )
        line-buffer over here swap          ( #r #c lb here #c )
        dup allot                           ( #r #c lb here #c )
        my-cmove                            ( #r #c )
        swap 1+ swap                        ( #r+1 #c )
    repeat drop                             ( #r #c )
    here 'cansee ! 2dup * allot             ( #r #c )
    #cols ! #rows !
;

: process       ( -- )
\    0 #rows !
\    0 #cols !
    create-arrays
    cr ." r c = " #rows ? #cols ? cr
    ." forrest addr = " 'forrest ? cr
    .s cr
    'forrest .array
;

: .result       ( -- )
    ." The answer is: " cr
;

: go
    open-input process .result close-input ;