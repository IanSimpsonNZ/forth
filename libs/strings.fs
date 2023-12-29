
: $split        ( addr len char -- addr1 len1 addr2 len2 )  \ find delim (char) in string - str1 ends before delim, str2 includes delim
                                                            \ str2 is empty (ie len 0) if delim not found in original string
    rot rot over + over                                 ( char addr end-addr c-addr )
    2swap swap 2swap                                    ( addr char end-addr c-addr )
    begin                                               ( addr char end-addr c-addr )
        2dup >                                          ( addr char end-addr c-addr f )
        2over drop rot dup c@ rot <> rot                ( addr char end-addr c-addr f f )
    and while                                           ( addr char end-addr c-addr )
        1+                                              ( addr char end-addr c-addr )
    repeat                                              ( addr char end-addr c-addr )
    dup rot swap -                                      ( addr char c-addr len2 )
    2swap drop rot 2dup swap -                          ( len2 addr c-addr len1 )
    rot swap 2swap swap                                 ( addr len1 c-addr len2 )
;

: $trim-front   ( addr len -- addr len )                \ remove leading spaces
    begin 2dup 0> swap c@ 32 = and while                ( addr len )
    1- swap 1+ swap repeat                              ( addr+1 len-1 )
;

: $tring    ( max-len -- )                              \ stores max len and current len - : max-len : current-len : char-data-> :
    create dup , 0 , allot
    does> ( -- addr len ) 1 cells + dup 1 cells + swap @
;

: $copy     ( addr1 len1 addr2 len2 -- )                \ usage $from-string $to-string $copy - assumes my $string format above for $to-string
    drop dup 2 cells - @ rot min                                  ( addr1 addr2 #chars )
    2dup swap 1 cells - !                               ( addr1 addr2 #chars )          \ store string length in target
    move ;                                              (  )                            \ copy chars from string1 up to max-length of string2


: $+        ( addr1 len1 addr2 len2 -- )                \ copy string 2 to the end of string 1
    2over 2over swap drop +                             ( addr1 len1 addr2 len2 addr1 new-len )
    over 2 cells - @                                    ( addr1 len1 addr2 len2 addr1 new-len str1-max-len )
    over < if -1 abort" String overflow in $+" then     ( addr1 len1 addr2 len2 addr1 new-len )
    swap 1 cells - !                                    ( addr1 len1 addr2 len2 )
    2swap + swap move                                ( addr2 len2 end-1 )
;

: $add-char ( addr len char -- addr len' )
    rot dup 1 cells - dup @ swap 1 cells - @            ( len char addr len max-len )
    < if                                                ( len char addr )
        rot over + rot swap !                           ( addr )
        1 cells - dup @ 1+ swap 2dup !                  ( len+1 addr-1c )
        1 cells + swap                                  ( addr len+1 )
    else
        -1 abort" String overflow"
    then
;

: $init     ( addr len -- )
    drop 1 cells - 0 swap !
;

24 $tring hello
10 $tring world
: test$+
    hello $init
    world $init
    s" Hello" hello drop 2dup 1 cells - ! swap move                 (  )
    s" , World" world drop 2dup 1 cells - ! swap move                 (  )
    cr
    hello .s type cr
    world .s type cr
    hello world $+
    hello .s type cr
    hello world $+
    hello .s type cr
    hello world $+
    hello .s type cr
;