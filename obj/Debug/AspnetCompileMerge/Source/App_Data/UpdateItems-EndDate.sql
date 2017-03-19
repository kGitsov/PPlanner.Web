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

 
   /****** Script for SelectTopNRows command from SSMS  ******/
 Update R
set R.PositionName = N'Developer'
 FROM dbo.ProjectDevelopers AS R
 where PositionName = N'	Developer	'

   update R
  set R.CompletedDate = CAST(J.EndDate AS DATE)
  from dbo.UserStories AS R
  inner join dbo.Sprints AS J
  ON Sprint_SprintId = J.SprintId
  Where CAST(J.EndDate AS DATE) <= CAST(GETDATE() AS DATE)

    update R
  set R.CompletedDate = J.EndDate
  from dbo.UserStories AS R
  inner join dbo.Sprints AS J
  ON Sprint_SprintId = J.SprintId
  Where J.EndDate <= GETDATE()

       update R
  set R.ItemStatus_ItemStatusId = 3
  from dbo.UserStories AS R
  inner join dbo.Sprints AS J
  ON Sprint_SprintId = J.SprintId
  Where J.EndDate <= GETDATE()


  update R
set R.CompletedDate = Dateadd(day,2,R.CompletedDate)
from dbo.UserStories AS R
where ItemStatus_ItemStatusId = 3 and User_Effort<=2

update R
set R.CompletedDate = DateAdd(day,-3,J.EndDate)
from dbo.UserStories AS R
inner join dbo.Sprints AS J
ON sprint_sprintid = sprintid
where ItemStatus_ItemStatusId = 3 and User_Effort<=8 and User_effort >3