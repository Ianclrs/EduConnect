import { useState, useEffect } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import toast from 'react-hot-toast';
import { useAuth } from '../hooks/useAuth';
import { setAccessToken } from '../api/client';
import { Button } from '../components/Button';
import { Input } from '../components/Input';
import { Card } from '../components/Card';

export default function LoginPage() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const { login, isAuthenticated, user, devLogin } = useAuth();
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();

  useEffect(() => {
    if (isAuthenticated && user) {
      navigate(user.role === 'Parent' ? '/parent' : '/admin', { replace: true });
    }
  }, [isAuthenticated, user, navigate]);

  useEffect(() => {
    const token = searchParams.get('token');
    if (token) {
      setAccessToken(token);
      window.location.href = '/admin';
    }
  }, [searchParams]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    try {
      await login(email, password);
      toast.success('Login realizado!');
    } catch {
      toast.error('Email ou senha inválidos.');
    } finally {
      setLoading(false);
    }
  };

  const handleGoogleLogin = () => {
    window.location.href = `${import.meta.env.VITE_API_URL || ''}/auth/google`;
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50 px-4">
      <Card className="w-full max-w-md">
        <div className="text-center mb-6">
          <h1 className="text-2xl font-bold text-indigo-600">EduGestor</h1>
          <p className="text-gray-500 mt-1">Sistema de Gestão Escolar</p>
        </div>
        <form onSubmit={handleSubmit} className="space-y-4">
          <Input label="Email" type="email" value={email} onChange={(e) => setEmail(e.target.value)} required />
          <Input label="Senha" type="password" value={password} onChange={(e) => setPassword(e.target.value)} required />
          <Button type="submit" className="w-full" disabled={loading}>
            {loading ? 'Entrando...' : 'Entrar'}
          </Button>
        </form>
        <div className="mt-4">
          <Button variant="secondary" className="w-full" onClick={handleGoogleLogin}>
            Entrar com Google
          </Button>
        </div>
        <div className="mt-6 pt-4 border-t border-gray-200">
          <p className="text-xs text-gray-400 text-center mb-2">Desenvolvimento</p>
          <div className="flex gap-2">
            <Button variant="secondary" size="sm" className="flex-1" onClick={() => devLogin('Admin')}>Admin</Button>
            <Button variant="secondary" size="sm" className="flex-1" onClick={() => devLogin('Parent')}>Pai</Button>
          </div>
        </div>
      </Card>
    </div>
  );
}
