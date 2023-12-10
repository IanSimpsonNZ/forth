require ~/forth/libs/files.fs
require ~/forth/libs/queue.fs
require ~/forth/libs/strings.fs

: test     ( -- file-name-addr file-name-len )
    s" test.txt" ;

: real     ( -- file-name-addr file-name-len )
    s" input.txt" ;

\ : array ( num-elements -- ) create cells allot
\    does> ( element-number -- addr ) swap cells + ;


512 Constant max-line
Create line-buffer  max-line 2 + allot

variable buff-len


512 constant max-q-len 
create sources max-q-len 1 cells q-create drop
create destinations max-q-len 1 cells q-create drop

: get-line      ( -- num-chars flag ) line-buffer max-line fd-in read-line throw ;
: get-next-num       ( str len -- n addr rem )
    0. 2swap >number rot drop 1- swap 1+ swap ;  ( rot drop removes top of double result )



: get-seeds     ( -- )
    get-line invert if -1 abort" Can't read from file" then         ( len )
    line-buffer swap [char] : $split                                ( addr1 len1 addr2 len2 )
    2swap 2drop                                                     ( addr2 len2 )
    1- swap 1+ swap $trim-front                                     ( addr len )
    begin dup 0> while                                              ( addr len )
        $trim-front
        get-next-num $trim-front get-next-num                       ( seed range addr rem )
        2swap over + swap                                           ( addr rem seed-end seed-start )
        do     FINE FOR SHORT RANGES, BUT NOT FOR REAL DEAL!
            i sources q-push                                        ( addr rem )
            i destinations q-push                                   ( addr rem )                \ destination defaults to source
        loop 
    repeat
    2drop

    get-line
    invert if -1 abort" No maps" then
    0> if -1 abort" Expected a blank line after the seed data" then
;

: get-ranges            ( len -- dest source range )
    line-buffer swap
    $trim-front get-next-num                    ( dest addr rem )
    $trim-front get-next-num                    ( dest source addr rem )
    $trim-front get-next-num                    ( dest source range addr rem )
    2drop                                       ( dest source range )
;

: trans             ( dest source range -- )
    over +                                      ( dest-start source-start source-end )
    sources q-len 0 do
        2dup sources i q-data-ptr @ dup         ( dest-start source-start source-end source-start source-end s s )
        rot < rot rot swap over <= rot and if   ( dest-start source-start source-end s )
            2over rot swap - +                  ( dest-start source-start source-end dest )
            destinations i q-data-ptr !         ( dest-start source-start source-end )
        else drop then                          ( dest-start source-start source-end )
    loop
    2drop drop
;

: process-map-lines      ( -- )
    begin get-line over 0> and while            ( len )
        get-ranges                              ( dest source length )
        trans                                   (  )
    repeat drop
;

: process-map   ( -- f )
    get-line dup if                             ( len f )
        swap dup 0> if                          ( f len )
            line-buffer swap type cr            ( f )
            process-map-lines                   ( f )
        else -1 abort" Expected map title line" then
    else swap drop then                         ( f )
;

: dest-to-source    ( -- )
    sources q-len 0 do
        destinations i q-data-ptr @ sources i q-data-ptr !
    loop
;


: process       ( -- )
    cr
    get-seeds
\    sources q. cr 
    begin process-map while dest-to-source repeat 
;

: .result       ( -- )
    sources q-len 0= if -1 abort" No data" then
    sources 0 q-data-ptr @                               ( n )
    sources q-len 0 do sources i q-data-ptr @ min loop    ( n )

    cr ." The minimum is " . cr
;

: setup         ( -- )
    sources q-init
    destinations q-init
;

: go
    open-input
    setup
    process 
    close-input
    .result
;
