: speller ( n -- ) \ |n| <= 4
   dup 0< swap \ set up negative flag
   abs dup 4 > if ." Out of range" 2drop exit then
   dup 0= if ." zero" 2drop else
     swap if ." negative " then
     dup 1 = if ." one" else
     dup 2 = if ." two" else
     dup 3 = if ." three" else ." four"
     then then then drop then ;