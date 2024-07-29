# all courses
- url: `http://win2.fh-timing.com/middleware/info/json?setting=courses`
- active as of 29.08.24

# all splits for a course
- url: `http://win2.fh-timing.com/middleware/info/json?setting=splits&course={coursenumber}`
- also ok as of 29.08.24

# categories for a course
- url: `http://win2.fh-timing.com/middleware/info/json?setting=categories&course=101`

# current results for course + splits
- url: `http://win2.fh-timing.com/middleware/2305270/result/json?course=102&detail=start,gender,status&splitnr=101,109,119,199`
- most probably '2304270' is some stage/version number, so proper url should not include it
- works but gives an empty result as of 29.08.24
- other details: category, age; first, last; club, team
- filter by category: `&categ=1-F`
