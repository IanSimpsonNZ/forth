: array ( num-elements -- ) create cells allot
    does> ( element-number -- addr ) swap cells + ;

: b-array ( num-elements -- ) create dup , cells allot ;
(    does> ( element-number -- addr  2dup @ < if swap cells + 1+ else -1 abort" b-array range overflow" then ; )
