: array ( num-elements -- ) create cells allot                      \ Simple array - usage "100 array myarray", then "el-num myarray @"
    does> ( element-number -- addr ) swap cells + ;

: b-array ( num-elements -- ) create dup , cells allot              \ Array with bound checks - usage "100 b-array myarray2", then "el-num myarray2 @"
    does> ( element-number -- addr ) 2dup @ over 0>= rot rot < and if swap 1+ cells + else -1 abort" b-array range overflow" then ;

