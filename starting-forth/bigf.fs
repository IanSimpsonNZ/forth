: star ( -- ) [char] * emit ;
: stars ( n -- ) 0 do star loop ;
: margin ( -- ) cr 30 spaces ;
: blip ( -- ) margin star ;
: bar ( -- ) margin 5 stars ;
: f ( -- ) bar blip bar blip blip cr ;
