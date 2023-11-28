require ~/forth/libs/files.fs

1024 8 * Constant max-line
Create line-buffer  max-line 2 + allot

14 constant marker-len

: test     ( -- file-name-addr file-name-len )
    s" test.txt" ;

: real     ( -- file-name-addr file-name-len )
    s" input.txt" ;

: get-line      ( buff -- n-chr flag )
    line-buffer max-line fd-in read-line throw ;

: check-dup     ( char start #chars -- 0|-1 )   \ 0 if no dup found
    rot 0 swap 2swap                    ( 0 c start #c )        \ default to no dup found
    over + swap do                      ( 0 c )
        dup i c@ = if
            swap drop -1 swap           ( -1 c )
            leave
        then
    loop
    drop
;

: marker?      ( c-pos -- 0|-1 )        \ 0 if dup found
    line-buffer +                       ( c-ptr )
    0 swap                              ( 0 c-ptr )                 \ default to duplicate ie not a marker
    marker-len 1- dup                   ( 0 c-ptr ml-1 ml-1 )
    0 do                                ( 0 c-ptr ml-1 )
        2dup 1- swap 1+ swap 2swap      ( 0 c-ptr+1 ml-2 c-ptr ml-1 )
        swap dup c@                     ( 0 c-ptr+1 ml-2 ml-1 c-ptr c )
        swap 1+                         ( 0 c-ptr+1 ml-2 ml-1 c c-ptr+1 )
        rot                             ( 0 c-ptr+1 ml-2 c c-ptr+1 ml-1 )
        check-dup                       ( 0 c-ptr+1 ml-2 flag )
        if
            rot drop -1 rot rot
            leave                       ( -1 c-ptr+1 ml-2 )
        then
    loop                                ( 0 c-ptr+1 ml-2 )
    2drop
;

: process       ( -- n|0)                   \ n = end of marker position or 0 if no marker found
    get-line invert if -1 abort" Could not read message " then
    marker-len -                            ( len - marker-len )
    dup 0<= if -1 abort" Message too short " then
    0 begin                                 ( len c-pos )
        2dup > while                        ( len c-pos )
        dup marker?                         ( len c-pos flag )
        while                               ( len c-pos )
        1+                                  ( len c-pos+1 )
    repeat then                             ( len c-pos )
    2dup = if 2drop 0 else marker-len + swap drop then
;

: .result       ( n|0 -- )
    dup 0= if
        ." No marker found " cr drop else
        ." The answer is: " . cr
    then
;

: go
    open-input process .result close-input ;