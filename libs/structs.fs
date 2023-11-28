
: dofield       \ the action of a field
does>           \ addr1 -- addr2 ; calculate the address of a field
    @ + ;

: field         \ # n ++ # ; define a field with offset # and size n
    create over , +     \ store offset, add the size to find new offset
    dofield ;


\ 0 
\ 5 chars field a
\ .s
\ 3 cells field b
\ .s
\ 2 cells field c
\ .s

\ constant #foo
\ .s
\ #foo create foo allot
\ .s
