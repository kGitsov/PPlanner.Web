/****** Script for SelectTopNRows command from SSMS  ******/


Update R
set R.ItemStatus_ItemStatusId = 3
 FROM dbo.UserStories AS R

 
/****** Script for SelectTopNRows command from SSMS  ******/


Update R
set R.CompletedDate = Sprints.EndDate
 FROM dbo.UserStories AS R
  inner join Sprints
  ON R.Sprint_SprintId = Sprints.SprintId
Where R.Sprint_SprintId = Sprints.SprintId

 
