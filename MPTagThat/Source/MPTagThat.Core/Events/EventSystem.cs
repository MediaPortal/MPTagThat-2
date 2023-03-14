#region Copyright (C) 2022 Team MediaPortal
// Copyright (C) 2022 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MPTagThat is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MPTagThat is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MPTagThat. If not, see <http://www.gnu.org/licenses/>.
#endregion

using Prism.Events;
using System;

namespace MPTagThat.Core
{
  public static class EventSystem
  {
    private static IEventAggregator _current;
    public static IEventAggregator Current => _current ?? (_current = new EventAggregator());

    private static PubSubEvent<TEvent> GetEvent<TEvent>()
    {
      return Current.GetEvent<PubSubEvent<TEvent>>();
    }

    public static void Publish<TEvent>()
    {
      Publish(default(TEvent));
    }

    public static void Publish<TEvent>(TEvent @event)
    {
      GetEvent<TEvent>().Publish(@event);
    }

    public static SubscriptionToken Subscribe<TEvent>(Action action, ThreadOption threadOption = ThreadOption.PublisherThread, bool keepSubscriberReferenceAlive = false)
    {
      return Subscribe<TEvent>(e => action(), threadOption, keepSubscriberReferenceAlive);
    }

    public static SubscriptionToken Subscribe<TEvent>(Action<TEvent> action, ThreadOption threadOption = ThreadOption.PublisherThread, bool keepSubscriberReferenceAlive = false, Predicate<TEvent> filter = null)
    {
      return GetEvent<TEvent>().Subscribe(action, threadOption, keepSubscriberReferenceAlive, filter);
    }

    public static void Unsubscribe<TEvent>(SubscriptionToken token)
    {
      GetEvent<TEvent>().Unsubscribe(token);
    }
    public static void Unsubscribe<TEvent>(Action<TEvent> subscriber)
    {
      GetEvent<TEvent>().Unsubscribe(subscriber);
    }
  }
}
