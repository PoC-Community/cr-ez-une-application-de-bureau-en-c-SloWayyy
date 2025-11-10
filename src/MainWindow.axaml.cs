using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using TodoListApp.Models;

namespace TodoListApp;

public partial class MainWindow : Window
{
    private ObservableCollection<TaskItem> _tasks = new();
    private readonly string _dataFilePath;

    public MainWindow()
    {
        InitializeComponent();
        
        // Set up data file path
        string dataDirectory = Path.Combine(Directory.GetCurrentDirectory(), "data");
        _dataFilePath = Path.Combine(dataDirectory, "tasks.json");
        
        // Load tasks on startup
        LoadTasks();
        
        TaskList.ItemsSource = _tasks;

        AddButton.Click += OnAddClick;
        DeleteButton.Click += OnDeleteClick;
        SaveButton.Click += OnSaveClick;
    }

    private void OnAddClick(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(TaskInput.Text))
        {
            _tasks.Add(new TaskItem { Title = TaskInput.Text });
            TaskInput.Text = string.Empty;
        }
    }

    private void OnDeleteClick(object? sender, RoutedEventArgs e)
    {
        if (TaskList.SelectedItem is TaskItem selected)
        {
            _tasks.Remove(selected);
        }
    }

    private void OnSaveClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            // Create data directory if it doesn't exist
            string? dataDirectory = Path.GetDirectoryName(_dataFilePath);
            if (!string.IsNullOrEmpty(dataDirectory) && !Directory.Exists(dataDirectory))
            {
                Directory.CreateDirectory(dataDirectory);
            }

            // Serialize tasks to JSON
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            string jsonString = JsonSerializer.Serialize(_tasks, options);

            // Write to file
            File.WriteAllText(_dataFilePath, jsonString);
        }
        catch (Exception ex)
        {
            // Handle errors (permissions, disk space, etc.)
            // Minimal error handling - could be improved with user notification
            System.Diagnostics.Debug.WriteLine($"Error saving tasks: {ex.Message}");
        }
    }

    private void LoadTasks()
    {
        try
        {
            if (File.Exists(_dataFilePath))
            {
                // Read JSON file
                string jsonString = File.ReadAllText(_dataFilePath);
                
                // Deserialize into a list of TaskItem
                List<TaskItem>? loadedTasks = JsonSerializer.Deserialize<List<TaskItem>>(jsonString);
                
                // Clear current list and add loaded tasks
                _tasks.Clear();
                if (loadedTasks != null)
                {
                    foreach (var task in loadedTasks)
                    {
                        _tasks.Add(task);
                    }
                }
            }
            else
            {
                // File doesn't exist - start with empty list
                _tasks.Clear();
            }
        }
        catch (JsonException ex)
        {
            // Handle corrupted JSON
            _tasks.Clear();
            System.Diagnostics.Debug.WriteLine($"Corrupted JSON file: {ex.Message}");
        }
        catch (Exception ex)
        {
            // Handle other errors (permissions, etc.)
            _tasks.Clear();
            System.Diagnostics.Debug.WriteLine($"Error loading tasks: {ex.Message}");
        }
    }
}
