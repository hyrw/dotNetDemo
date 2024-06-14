using CommunityToolkit.Mvvm.ComponentModel;
using DragDemo.Models;
using System.Collections.ObjectModel;

namespace DragDemo.ViewModels;

public partial class TodoViewModel : ObservableObject
{

    [ObservableProperty]
    ObservableCollection<TodoItemModel> _completes;
    
    [ObservableProperty]
    ObservableCollection<TodoItemModel> _unCompletes;

    [ObservableProperty]
    TodoListViewModel _completeViewModel;

    [ObservableProperty]
    TodoListViewModel _unCompleteViewModel;

    public TodoViewModel()
    {
        _completes =
        [
            new TodoItemModel{Id = "3", Desc = "complete 3"},
            new TodoItemModel{Id = "4", Desc = "complete 4"},
        ];

        _unCompletes =
        [
            new TodoItemModel{Id = "1", Desc = "todo 1"},
            new TodoItemModel{Id = "2", Desc = "todo 2"},
        ];

        _completeViewModel = new(_completes);
        _unCompleteViewModel = new(_unCompletes);
    }
}
