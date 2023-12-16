require ~/forth/libs/structs.fs

0
1 cells field #bt-data
1 cells field bt-left           ( 0 => no link )
1 cells field bt-right          ( 0 => no link )
0 field bt-data
constant #bt-header

: bt-init        ( bt-addr -- )
    dup #bt-data @                          ( addr #data )
    over bt-data swap erase                 ( addr )
    dup 0 swap bt-left !                    ( addr )
    0 swap bt-right !
;

: q-create      ( data-size -- addr )              \ usage: data-size-bytes bt-create tree-ptr !
    here over #bt-header + allot            ( data-size here )
    swap over #bt-data !                    ( here )
    dup bt-init                             ( here )
;

: bt-info        ( q-addr -- )
    ." Node address     : " dup . cr
    ." Data len(bytes)  : " dup #bt-data . ."  contains: " dup #bt-data ? cr
    ." Left pointer at  : " dup bt-left .   ."  contains: " dup bt-left ? cr
    ." Right pointer at : " dup bt-right  .   ."  contains: " dup bt-right ? cr
    ." Data starts at   : " #bt-header + . cr
;


