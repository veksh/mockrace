# all courses
- url: `http://win2.fh-timing.com/middleware/info/json?setting=courses`
- active as of 29.08.24, results: like
``` json
{
  "Courses": [
    {
      "Coursenr": "104",
      "Coursename": "ETAPPE4",
      "Event": "2409200",
      "Eventname": "TDKGRAVEL 2024",
      "Status": "1",
      "Timeoffset": "7200",
      "Ordering": "170",
      "Remark": ""
    }, {}
  ]
}
```

# all splits for a course
- url: `http://win2.fh-timing.com/middleware/info/json?setting=splits&course=104`
- ok as of 29.08.24, results: like
``` json
{
  "Splits_104": [
    {
      "Splitnr": "101",
      "Splitname": "START_ET4",
      "ID": "1",
      "State": "",
      "ToD": "1"
    }, {}
  ]
}
```
- only numbers less than 999 are of interest

# categories for a course
- url: `http://win2.fh-timing.com/middleware/info/json?setting=categories&course=104`
- ok as of 29.08.24, results: like
``` json
{
  "Categories_101": [
    {
      "Category": "0-?"
    }, {}
  ]
}
```

# current results for course + splits
- url: `http://win2.fh-timing.com/middleware/2305270/result/json?course=102&detail=start,gender,first,last,status&splitnr=101,109,119,199`
- most probably '2304270' is some stage/version number, so proper url should not include it
- works but gives an empty result as of 29.08.24
- other details: category, age; club, team
- filter by category: `&categ=1-F`
- filter by field: `&filter=gender:W.*`
- top 10 at split 199: `&rank=199&splitnr=199&count=10`
- expected answer is like
``` json
{
  "Course_102": [
    {
      "start": "1",
      "gender": "W",
      "first": "Jana",
      "last": "Gigele",
      "status": "-",
      "START_CHECK_ET2_Time": "00:00:01.5",
      "TURN1_ET2_Time": "00:07:51.2",
      "TURN2_ET2_Time": "00:17:28.9",
      "FINISH_ET2_Time": "00:26:18.8"
    }, {}
  ]
}
```
