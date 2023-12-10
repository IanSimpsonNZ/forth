
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

: $trim-front   ( addr len -- addr len )                    \ remove leading spaces
    begin 2dup 0> swap c@ 32 = and while                ( addr len )
    1- swap 1+ swap repeat                              ( addr+1 len-1 )
;
