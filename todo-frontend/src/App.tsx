import axios from 'axios';
import React, {useState} from 'react';

interface Todo {
  id: string;
  title: string;
  description?: string;
  completed: boolean;
}

const api = axios.create({
  baseURL: 'http://localhost:61278/api/todos',
  headers: {'Content-Type': 'application/json'},
});

export default function App() {
  const [todos, setTodos] = useState<Todo[]>([]);
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [selectedId, setSelectedId] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const fetchTodo = async (id: string) => {
    try {
      const res = await api.get<Todo>(`/${id}`);
      setTodos([res.data]);
    } catch {
      alert('Todo not found');
    }
  };

  const createTodo = async () => {
    if (!title.trim()) return;
    setLoading(true);
    try {
      const res = await api.post('/', {title, description});
      const {id} = res.data;
      const newTodo = await api.get<Todo>(`/${id}`);
      setTodos([...todos, newTodo.data]);
      setTitle('');
      setDescription('');
    } catch {
      alert('Error creating todo');
    } finally {
      setLoading(false);
    }
  };

  const updateTodo = async (id: string) => {
    try {
      await api.put(`/${id}`, {title, description});
      const updated = await api.get<Todo>(`/${id}`);
      setTodos(todos.map((t) => (t.id === id ? updated.data : t)));
      setSelectedId(null);
    } catch {
      alert('Error updating todo');
    }
  };

  const completeTodo = async (id: string) => {
    await api.post(`/${id}/complete`);
    const updated = await api.get<Todo>(`/${id}`);
    setTodos(todos.map((t) => (t.id === id ? updated.data : t)));
  };

  const deleteTodo = async (id: string) => {
    await api.delete(`/${id}`);
    setTodos(todos.filter((t) => t.id !== id));
  };

  return (
    <div className='min-h-screen bg-gray-50 p-6 flex flex-col items-center'>
      <h1 className='text-3xl font-bold mb-6'>Todo Frontend</h1>

      <div className='border rounded-lg bg-white shadow p-4 mb-6 w-full max-w-xl'>
        <input
          type='text'
          placeholder='Title'
          value={title}
          onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
            setTitle(e.target.value)
          }
          className='border p-2 rounded w-full mb-2'
        />
        <br />
        <br />
        <textarea
          placeholder='Description'
          value={description}
          onChange={(e: React.ChangeEvent<HTMLTextAreaElement>) =>
            setDescription(e.target.value)
          }
          className='border p-2 rounded w-full mb-2'
        />
        <div className='flex gap-2'>
          <button
            onClick={createTodo}
            disabled={loading}
            className='px-3 py-2 bg-blue-600 text-white rounded'
          >
            {loading ? 'Creating...' : 'Create Todo'}
          </button>
          {selectedId && (
            <button
              onClick={() => updateTodo(selectedId)}
              className='px-3 py-2 bg-green-600 text-white rounded'
            >
              Update Todo
            </button>
          )}
        </div>
      </div>

      <div className='w-full max-w-xl space-y-3'>
        {todos.map((todo) => (
          <div
            key={todo.id}
            className='border rounded-lg bg-white shadow p-4 flex justify-between items-center'
          >
            <div>
              <h2
                className={`text-xl font-semibold ${
                  todo.completed ? 'line-through text-gray-500' : ''
                }`}
              >
                {todo.title}
              </h2>
              <p className='text-gray-600'>{todo.description}</p>
            </div>
            <div className='flex gap-2'>
              <button
                onClick={() => {
                  setSelectedId(todo.id);
                  setTitle(todo.title);
                  setDescription(todo.description || '');
                }}
                className='px-2 py-1 border rounded'
              >
                Edit
              </button>
              <button
                onClick={() => completeTodo(todo.id)}
                disabled={todo.completed}
                className='px-2 py-1 border rounded'
              >
                Complete
              </button>
              <button
                onClick={() => deleteTodo(todo.id)}
                className='px-2 py-1 border rounded bg-red-600 text-white'
              >
                Delete
              </button>
            </div>
          </div>
        ))}
        {todos.length === 0 && (
          <p className='text-center text-gray-500'>No todos yet</p>
        )}
      </div>

      <div className='mt-6 flex gap-2'>
        <input
          placeholder='Fetch Todo by ID...'
          value={selectedId || ''}
          onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
            setSelectedId(e.target.value)
          }
          className='border p-2 rounded'
        />
        <button
          onClick={() => selectedId && fetchTodo(selectedId)}
          className='px-3 py-2 bg-gray-600 text-white rounded'
        >
          Fetch
        </button>
      </div>
    </div>
  );
}
