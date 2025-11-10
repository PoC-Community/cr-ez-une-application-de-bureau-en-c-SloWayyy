using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using TodoListApp.Models;

namespace TodoListApp;

public partial class MainWindow : Window
{
    private ObservableCollection<TaskItem> _tasks = new();

    public MainWindow()
    {
        InitializeComponent();
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
            string dataDirectory = Path.Combine(Directory.GetCurrentDirectory(), "data");
            if (!Directory.Exists(dataDirectory))
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
            string filePath = Path.Combine(dataDirectory, "tasks.json");
            File.WriteAllText(filePath, jsonString);
        }
        catch (Exception ex)
        {
            // Handle errors (permissions, disk space, etc.)
            // Minimal error handling - could be improved with user notification
            System.Diagnostics.Debug.WriteLine($"Error saving tasks: {ex.Message}");
        }
    }
}
