using System.Collections.Generic;
using System.Data.SqlClient;
using System;

namespace ToDoList
{
  public class Task
  {
    private int _id;
    private string _description;
    private bool _completed;

    public Task(string Description, bool Completed = false, int Id = 0)
    {
      _id = Id;
      _description = Description;
      _completed = Completed;
    }

		public override bool Equals(Object otherTask)
		{
			if (!(otherTask is Task))
			{
				return false;
			}
			else
			{
				Task newTask = (Task) otherTask;
				bool idEquality = (this.GetId() == newTask.GetId());
				bool descriptionEquality = (this.GetDescription() == newTask.GetDescription());
        bool completedEquality = (this.GetCompleted() == newTask.GetCompleted());
				return (idEquality && descriptionEquality && completedEquality);
			}
		}

    public int GetId()
    {
      return _id;
    }
    public string GetDescription()
    {
      return _description;
    }
    public void SetDescription(string newDescription)
    {
      _description = newDescription;
    }
    public static List<Task> GetAll()
    {
      List<Task> allTasks = new List<Task>{};

      SqlConnection conn = DB.Connection();
      conn.Open();

      SqlCommand cmd = new SqlCommand("SELECT * FROM tasks;", conn);
      SqlDataReader rdr = cmd.ExecuteReader();

      while(rdr.Read())
      {
        int taskId = rdr.GetInt32(0);
        string taskDescription = rdr.GetString(1);
        bool taskCompleted = rdr.GetBoolean(2);
        Task newTask = new Task(taskDescription, taskCompleted, taskId);
        allTasks.Add(newTask);
      }

      if (rdr != null)
      {
        rdr.Close();
      }
      if (conn != null)
      {
        conn.Close();
      }

      return allTasks;
    }
/////OVERLOAD TO RETRIEVE ONLY COMPLETED OR UNCOMPLETED
    public static List<Task> GetAll(bool status)
    {
      ///true status returns only completed tasks, false status returns incomplete tasks
      List<Task> allTasks = new List<Task>{};

      SqlConnection conn = DB.Connection();
      conn.Open();

      SqlCommand cmd = new SqlCommand("SELECT * FROM tasks WHERE completed = @Completed;", conn);
      cmd.Parameters.AddWithValue("@Completed", status);
      SqlDataReader rdr = cmd.ExecuteReader();

      while(rdr.Read())
      {
        int taskId = rdr.GetInt32(0);
        string taskDescription = rdr.GetString(1);
        bool taskCompleted = rdr.GetBoolean(2);
        Task newTask = new Task(taskDescription, taskCompleted, taskId);
        allTasks.Add(newTask);
      }

      if (rdr != null)
      {
        rdr.Close();
      }
      if (conn != null)
      {
        conn.Close();
      }

      return allTasks;
    }
/////
		public void Save()
		{
			SqlConnection conn = DB.Connection();
			conn.Open();
			SqlCommand cmd = new SqlCommand("INSERT INTO tasks (description, completed) OUTPUT INSERTED.id VALUES (@TaskDescription, @TaskCompleted);", conn);

      cmd.Parameters.AddWithValue("@TaskDescription", this.GetDescription());
      cmd.Parameters.AddWithValue("@TaskCompleted", this.GetCompleted());


			SqlDataReader rdr = cmd.ExecuteReader();

			while(rdr.Read())
			{
				this._id = rdr.GetInt32(0);
			}
			if (rdr != null)
			{
				rdr.Close();
			}
			if (conn != null)
			{
				conn.Close();
			}
		}

		public static Task Find(int id)
		{
			SqlConnection conn = DB.Connection();
			conn.Open();

			SqlCommand cmd = new SqlCommand("SELECT * FROM tasks WHERE id = @TaskId;", conn);

      cmd.Parameters.AddWithValue("@TaskId",id.ToString());
			SqlDataReader rdr = cmd.ExecuteReader();

			int foundTaskId = 0;
			string foundTaskDescription = null;
      bool foundTaskCompleted = false;

			while(rdr.Read())
			{
				foundTaskId = rdr.GetInt32(0);
				foundTaskDescription = rdr.GetString(1);
        foundTaskCompleted = rdr.GetBoolean(2);
			}
			Task foundTask = new Task(foundTaskDescription, foundTaskCompleted, foundTaskId);

			if (rdr != null)
			{
				rdr.Close();
			}
			if (conn != null)
			{
				conn.Close();
			}

			return foundTask;
		}

    public void AddCategory(Category newCategory)
    {
      SqlConnection conn = DB.Connection();
      conn.Open();

      SqlCommand cmd = new SqlCommand("INSERT INTO categories_tasks (category_id, task_id) VALUES (@CategoryId, @TaskId);", conn);

      cmd.Parameters.AddWithValue("@CategoryId", newCategory.GetId());
      cmd.Parameters.AddWithValue("@TaskId", this.GetId());

      cmd.ExecuteNonQuery();

      if (conn != null)
      {
        conn.Close();
      }
    }

    public List<Category> GetCategories()
    {
      SqlConnection conn = DB.Connection();
      conn.Open();

      SqlCommand cmd = new SqlCommand("SELECT category_id FROM categories_tasks WHERE task_id = @TaskId;", conn);
      cmd.Parameters.AddWithValue("@TaskId", this.GetId());
      SqlDataReader rdr = cmd.ExecuteReader();

      List<int> categoryIds = new List<int> {};

      while(rdr.Read())
      {
        int categoryId = rdr.GetInt32(0);
        categoryIds.Add(categoryId);
      }
      if (rdr != null)
      {
        rdr.Close();
      }

      List<Category> categories = new List<Category> {};

      foreach (int categoryId in categoryIds)
      {
        SqlCommand categoryQuery = new SqlCommand("SELECT * FROM categories WHERE id = @CategoryId;", conn);

        categoryQuery.Parameters.AddWithValue("@CategoryId", categoryId);
        SqlDataReader queryReader = categoryQuery.ExecuteReader();
        while (queryReader.Read())
        {
          int thisCategoryId = queryReader.GetInt32(0);
          string thisCategoryName = queryReader.GetString(1);
          Category foundCategory = new Category(thisCategoryName, thisCategoryId);
          categories.Add(foundCategory);
        }
        if (queryReader != null)
        {
          queryReader.Close();
        }
      }
      if (conn != null)
      {
        conn.Close();
      }
      return categories;
    }

    public void Delete()
    {
      SqlConnection conn = DB.Connection();
      conn.Open();

      SqlCommand cmd = new SqlCommand("DELETE FROM tasks WHERE id = @TaskId; DELETE FROM categories_tasks WHERE task_id = @TaskId;", conn);
      cmd.Parameters.AddWithValue("@TaskId", this.GetId());

      cmd.ExecuteNonQuery();

      if (conn != null)
      {
        conn.Close();
      }
    }

		public static void DeleteAll()
		{
		  SqlConnection conn = DB.Connection();
		  conn.Open();
		  SqlCommand cmd = new SqlCommand("DELETE FROM tasks;", conn);
		  cmd.ExecuteNonQuery();
		  conn.Close();
		}

    public static void Complete(int taskId)
    {
      SqlConnection conn = DB.Connection();
      conn.Open();
      SqlCommand cmd = new SqlCommand("UPDATE tasks SET completed = 1 WHERE id = @Id;", conn);
      cmd.Parameters.AddWithValue("@Id", taskId);

      cmd.ExecuteNonQuery();
      if (conn != null)
      {
        conn.Close();
      }
    }

    public void SetCompleted(bool newValue)
    {
      _completed = newValue;
    }
    public bool GetCompleted()
    {
      return _completed;
    }
  }
}
