using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
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
        CompleteAllButton.Click += OnCompleteAllClick;
        ClearCompletedButton.Click += OnClearCompletedClick;
        
        // Auto-save on collection changes
        _tasks.CollectionChanged += (s, e) => 
        {
            AutoSave();
            // Attach property change handlers to new items
            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    if (item is TaskItem task)
                    {
                        task.PropertyChanged += (sender, args) => AutoSave();
                    }
                }
            }
        };
        
        // Attach property change handlers to existing tasks
        foreach (var task in _tasks)
        {
            task.PropertyChanged += (sender, args) => AutoSave();
        }
    }

    private void OnAddClick(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(TaskInput.Text))
        {
            var newTask = new TaskItem 
            { 
                Title = TaskInput.Text,
                Tags = TagsInput.Text?.Trim() ?? string.Empty,
                DueDate = DueDatePicker.SelectedDate?.DateTime
            };
            _tasks.Add(newTask);
            TaskInput.Text = string.Empty;
            TagsInput.Text = string.Empty;
            DueDatePicker.SelectedDate = null;
            AutoSave();
        }
    }

    private void OnDeleteClick(object? sender, RoutedEventArgs e)
    {
        if (TaskList.SelectedItem is TaskItem selected)
        {
            _tasks.Remove(selected);
            AutoSave();
        }
    }

    private void OnCompleteAllClick(object? sender, RoutedEventArgs e)
    {
        foreach (var task in _tasks)
        {
            task.IsCompleted = true;
        }
        AutoSave();
    }

    private void OnClearCompletedClick(object? sender, RoutedEventArgs e)
    {
        var completedTasks = _tasks.Where(t => t.IsCompleted).ToList();
        foreach (var task in completedTasks)
        {
            _tasks.Remove(task);
        }
        AutoSave();
    }

    private void AutoSave()
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
            
            // Show save status
            SaveStatusText.IsVisible = true;
            Task.Delay(2000).ContinueWith(_ => 
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(() => SaveStatusText.IsVisible = false);
            });
        }
        catch (Exception ex)
        {
            // Handle errors silently for auto-save
            System.Diagnostics.Debug.WriteLine($"Error auto-saving tasks: {ex.Message}");
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
