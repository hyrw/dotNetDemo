using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DragDemo.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace DragDemo.ViewModels;

public class TodoListViewModel : ObservableObject
{
    private readonly ObservableCollection<TodoItemModel> _items;

    public ICommand AddCommand { get; }
    public ICommand RemoveCommand { get; }
    public TodoListViewModel(ObservableCollection<TodoItemModel> items)
    {
        _items = items;
        AddCommand = new RelayCommand<TodoItemModel>(Add);
        RemoveCommand = new RelayCommand<TodoItemModel>(Remove);
    }

    private void Add(TodoItemModel? item)
    {
        if (item is null || _items.Contains(item)) return;
        _items.Add(item);
    }

    private void Remove(TodoItemModel? item)
    {
        if (item is null) return;
        _items.Remove(item);
    }

}
