\ Use from prompt - time: myfunc

: time: ( "word" -- )
  utime 2>R ' EXECUTE
  utime 2R> D-
  <# # # # # # # [CHAR] . HOLD #S #> TYPE ."  seconds" ;
  