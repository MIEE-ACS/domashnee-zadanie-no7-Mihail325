using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Snake
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Поле на котором живет змея
        Entity field;
        // голова змеи
        Head head;
        // вся змея
        List<PositionedEntity> snake;
        // яблоко
        Apple apple;
        //количество очков
        int score;
        //таймер по которому 
        DispatcherTimer moveTimer;
        //для вызова различныыйх телепортов
        static int varportal=0;
        //таймер телепортов
        DispatcherTimer teleportTimercreat;
        //вспомогательные величины для телепортов
        bool creatclose = false;
        bool portal = false;
        //список телепортов
        List<Teleport> Portals= new List<Teleport>();
        //конструктор формы, выполняется при запуске программы
        public MainWindow()
        {
            InitializeComponent();
            
            snake = new List<PositionedEntity>();
            //создаем поле 300х300 пикселей
            field = new Entity(600, 600, "pack://application:,,,/Resources/snake.png");

            //создаем таймер срабатывающий раз в 300 мс
            moveTimer = new DispatcherTimer();
            moveTimer.Interval = new TimeSpan(0, 0, 0, 0, 300);
            moveTimer.Tick += new EventHandler(moveTimer_Tick);
            //создаем таймер срабатывающий раз в 8 с
            teleportTimercreat = new DispatcherTimer();
            teleportTimercreat.Interval = new TimeSpan(0, 0, 0, 8);
            teleportTimercreat.Tick += new EventHandler(teleportTimer_Tickcreat);
        }

        //метод перерисовывающий экран
        private void UpdateField()
        {
            //обновляем положение элементов змеи
            foreach (var p in snake)
            {
                Canvas.SetTop(p.image, p.y);
                Canvas.SetLeft(p.image, p.x);
            }

            //обновляем положение яблока
            Canvas.SetTop(apple.image, apple.y);
            Canvas.SetLeft(apple.image, apple.x);

            //обновляем количество очков
            lblScore.Content = String.Format("{0}000", score);
        }
        //обработчик таймера телепортов
        void teleportTimer_Tickcreat(object sender, EventArgs e) 
        {
            if (creatclose == false)
            {
                portal = false;
                foreach(var t in Portals)
                {
                    canvas1.Children.Remove(t.image1);
                    canvas1.Children.Remove(t.image2);
                }
                creatclose = true;
            }
            else 
            {
                portal = true;
                creatclose = false;
                int i = 0;
                foreach (var t in Portals)
                {
                    t.MoveTeleport(i);
                    Canvas.SetTop(t.image1, t.Py1);
                    Canvas.SetLeft(t.image1, t.Px1);
                    Canvas.SetTop(t.image2, t.Py2);
                    Canvas.SetLeft(t.image2, t.Px2);
                    canvas1.Children.Add(t.image1);
                    canvas1.Children.Add(t.image2);
                    i++;
                }
            }

        }
        void moveTimer_Tick(object sender, EventArgs e)
        {
            //в обратном порядке двигаем все элементы змеи
            foreach (var p in Enumerable.Reverse(snake))
            {
                p.move();
            }

            //проверяем, что голова змеи не врезалась в тело
            foreach (var p in snake.Where(x => x != head))
            {
                //если координаты головы и какой либо из частей тела совпадают
                if (p.x == head.x && p.y == head.y)
                {
                    //мы проиграли
                    moveTimer.Stop();
                    teleportTimercreat.Stop();
                    tbGameOver.Visibility = Visibility.Visible;
                    button1.Content = "Again?";
                    button1.Visibility = Visibility.Visible;
                    return;
                }
            }

            //проверяем, что голова змеи не вышла за пределы поля
            if (head.x < 40 || head.x >= 540 || head.y < 40 || head.y >= 540)
            {
                //мы проиграли
                moveTimer.Stop();
                teleportTimercreat.Stop();
                tbGameOver.Visibility = Visibility.Visible;
                button1.Content = "Again?";
                button1.Visibility = Visibility.Visible;
                return;
            }

            //проверяем, что голова змеи врезалась в яблоко
            if (head.x == apple.x && head.y == apple.y)
            {
                //увеличиваем счет
                score++;
                //двигаем яблоко на новое место
                apple.move();
                // добавляем новый сегмент к змее
                var part = new BodyPart(snake.Last());
                canvas1.Children.Add(part.image);
                snake.Add(part);
            }
            //проверяем поподание головы в один из телепортов
            if (portal==true)
            {
                foreach (var t in Portals)
                {
                    if (head.x == t.Px1 && head.y == t.Py1)
                    {
                        teleportTimercreat.Stop();
                        head.teleport(t.Px2, t.Py2);
                        foreach (var p in Enumerable.Reverse(snake))
                        {
                            p.move();
                        }
                        teleportTimercreat.Start();
                    }
                    if (head.x == t.Px2 && head.y == t.Py2)
                    {
                        teleportTimercreat.Stop();
                        head.teleport(t.Px1, t.Py1);
                        foreach (var p in Enumerable.Reverse(snake))
                        {
                            p.move();
                        }
                        teleportTimercreat.Start();
                    }
                }
            }
                //перерисовываем экран
                UpdateField();
        }

        // Обработчик нажатия на кнопку клавиатуры
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                    head.direction = Head.Direction.UP;
                    break;
                case Key.Down:
                    head.direction = Head.Direction.DOWN;
                    break;
                case Key.Left:
                    head.direction = Head.Direction.LEFT;
                    break;
                case Key.Right:
                    head.direction = Head.Direction.RIGHT;
                    break;
            }
        }

        // Обработчик нажатия кнопки "Start"
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            // обнуляем счет
            score = 0;
            // обнуляем змею
            snake.Clear();
            // очищаем канвас
            canvas1.Children.Clear();
            // скрываем надпись "Game Over"
            tbGameOver.Visibility = Visibility.Hidden;
            button1.Visibility = Visibility.Hidden;
            // добавляем поле на канвас
            canvas1.Children.Add(field.image);
            // создаем новое яблоко и добавлем его
            apple = new Apple(snake);
            canvas1.Children.Add(apple.image);
            // создаем голову
            head = new Head();
            snake.Add(head);
            canvas1.Children.Add(head.image);
            //запускаем таймер
            for (int i=0; i < 4; i++) 
            {
                Teleport teleport = new Teleport(snake, apple,Portals);
                Portals.Add(teleport);
            }
            moveTimer.Start();
            teleportTimercreat.Start();
            UpdateField();

        }
        
        public class Entity
        {
            protected int m_width;
            protected int m_height;
            
            Image m_image;
            public Entity(int w, int h, string image)
            {
                m_width = w;
                m_height = h;
                m_image = new Image();
                m_image.Source = (new ImageSourceConverter()).ConvertFromString(image) as ImageSource;
                m_image.Width = w;
                m_image.Height = h;

            }

            public Image image
            {
                get
                {
                    return m_image;
                }
            }
        }
        public class PositionedEntity : Entity
        {
            protected int m_x;
            protected int m_y;
            public PositionedEntity(int x, int y, int w, int h, string image)
                : base(w, h, image)
            {
                m_x = x;
                m_y = y;
            }

            public virtual void move() { }

            public int x
            {
                get
                {
                    return m_x;
                }
                set
                {
                    m_x = value;
                }
            }

            public int y
            {
                get
                {
                    return m_y;
                }
                set
                {
                    m_y = value;
                }
            }
        }
        // по идеи класс должен был наследоваться, но я позздно понял как нужно было решить проблему с двумя изображениями
        //Cаnvans возмущался на добавлении второго одинакового изображения
        public class Teleport
        {
            List<PositionedEntity> m_snake;
            Apple m_apple;
            List<Teleport> m_teleports;
            int m_x1;
            int m_x2;
            int m_y1;
            int m_y2;
            int m_width;
            int m_height;
            Image m_image1;
            Image m_image2;
            static string[] ArrayImage = new string[4] { "pack://application:,,,/Resources/Portal11.png", "pack://application:,,,/Resources/Portal21.png", "pack://application:,,,/Resources/Portal31.png", "pack://application:,,,/Resources/Portal41.png" };
            public Teleport(List<PositionedEntity> s, Apple apple,List<Teleport> teleports)
            {
                m_teleports = teleports;
                m_snake = s;
                m_apple = apple;
                m_width = 40;
                m_height = 40;
                m_x1 = 40;
                m_x2 = 400;
                m_y1 = 40;
                m_y2 = 400;
                m_image1 = new Image();
                m_image1.Source = (new ImageSourceConverter()).ConvertFromString(ArrayImage[varportal]) as ImageSource;
                m_image1.Width = m_width;
                m_image1.Height = m_height;
                m_image2 = new Image();
                m_image2.Source = (new ImageSourceConverter()).ConvertFromString(ArrayImage[varportal]) as ImageSource;
                m_image2.Width = m_width;
                m_image2.Height = m_height;
                if (varportal < 3)
                    varportal++;
                else
                    varportal = 0;
            }
            public void MoveTeleport(int i)
            {
                Random rand1 = new Random();
                do
                {
                    Px1 = rand1.Next(13) * 40 + 40;
                    Py1 = rand1.Next(13) * 40 + 40;
                    Px2 = rand1.Next(13) * 40 + 40;
                    Py2 = rand1.Next(13) * 40 + 40;
                    bool overlap1 = false;
                    bool overlap2 = false;
                    foreach (var p in m_snake)
                    {
                        for(int j=0; j<4;j++)
                        {
                            if (j != i)
                            {
                                if ((p.x == Px1 && p.y == Py1) || (m_apple.x == Px1 && m_apple.y == Py1) || (m_teleports[j].Px1 == Px1 && m_teleports[j].Py1 == Py1))
                                {
                                    overlap1 = true;
                                    break;
                                }
                                if ((p.x == Px2 && p.y == Py2) || (Px1 == Px2 && Py1 == Py2) || (m_apple.x == Px2 && m_apple.y == Py2) || (m_teleports[j].Px2 == Px2 && m_teleports[j].Py2 == Py2))
                                {
                                    overlap2 = true;
                                    break;
                                }
                            }
                        }
                    }
                    if (!overlap1 && !overlap2)
                        break;
                } while (true);
            }
            public Image image1
            {
                get
                {
                    return m_image1;
                }
            }
            public Image image2
            {
                get
                {
                    return m_image2;
                }
            }
            public int Px1
            {
                get
                {
                    return m_x1;
                }
                set
                {
                    m_x1 = value;
                }
            }
            public int Px2
            {
                get
                {
                    return m_x2;
                }
                set
                {
                    m_x2 = value;
                }
            }
            public int Py1
            {
                get
                {
                    return m_y1;
                }
                set
                {
                    m_y1 = value;
                }
            }
            public int Py2
            {
                get
                {
                    return m_y2;
                }
                set
                {
                    m_y2 = value;
                }
            }
        }


        public class Apple : PositionedEntity
        {
            List<PositionedEntity> m_snake;
            public Apple(List<PositionedEntity> s)
                : base(0, 0, 40, 40, "pack://application:,,,/Resources/fruit.png")
            {
                m_snake = s;
                move();
            }

            public override void move()
            {
                Random rand = new Random();
                do
                {
                    x = rand.Next(13) * 40 + 40;
                    y = rand.Next(13) * 40 + 40;
                    bool overlap = false;
                    foreach (var p in m_snake)
                    {
                        if (p.x == x && p.y == y)
                        {
                            overlap = true;
                            break;
                        }
                    }
                    if (!overlap)
                        break;
                } while (true);

            }
        }

        public class Head : PositionedEntity
        {
            public enum Direction
            {
                RIGHT, DOWN, LEFT, UP, NONE
            };

            Direction m_direction;

            public Direction direction {
                set
                {
                    m_direction = value;
                    RotateTransform rotateTransform = new RotateTransform(90 * (int)value+90);
                    image.RenderTransform = rotateTransform;
                }
            }

            public Head()
                : base(280, 280, 40, 40, "pack://application:,,,/Resources/head2.png")
            {
                image.RenderTransformOrigin = new Point(0.5, 0.5);
                m_direction = Direction.NONE;
            }

            public override void move()
            {
                switch (m_direction)
                {
                    case Direction.DOWN:
                        y += 40;
                        break;
                    case Direction.UP:
                        y -= 40;
                        break;
                    case Direction.LEFT:
                        x -= 40;
                        break;
                    case Direction.RIGHT:
                        x += 40;
                        break;
                }
            }
            public void teleport(int xt,int yt) 
            {
                x = xt;
                y = yt;
            }
        }

        public class BodyPart : PositionedEntity
        {
            PositionedEntity m_next;
            public BodyPart(PositionedEntity next)
                : base(next.x, next.y, 40, 40, "pack://application:,,,/Resources/Chank.png")
            {
                m_next = next;
            }

            public override void move()
            {
                x = m_next.x;
                y = m_next.y;
            }
        }


    }
}
