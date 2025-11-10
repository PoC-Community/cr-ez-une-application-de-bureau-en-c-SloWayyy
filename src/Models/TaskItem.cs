using System;
using System.ComponentModel;

namespace TodoListApp.Models;

public class TaskItem : INotifyPropertyChanged
{
    private bool _isCompleted;
    private string _title = string.Empty;
    private string _tags = string.Empty;
    private DateTime? _dueDate;

    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public string Title
    {
        get => _title;
        set
        {
            if (_title != value)
            {
                _title = value;
                OnPropertyChanged(nameof(Title));
            }
        }
    }
    
    public bool IsCompleted
    {
        get => _isCompleted;
        set
        {
            if (_isCompleted != value)
            {
                _isCompleted = value;
                OnPropertyChanged(nameof(IsCompleted));
            }
        }
    }
    
    public string Tags
    {
        get => _tags;
        set
        {
            if (_tags != value)
            {
                _tags = value;
                OnPropertyChanged(nameof(Tags));
            }
        }
    }
    
    public DateTime? DueDate
    {
        get => _dueDate;
        set
        {
            if (_dueDate != value)
            {
                _dueDate = value;
                OnPropertyChanged(nameof(DueDate));
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
