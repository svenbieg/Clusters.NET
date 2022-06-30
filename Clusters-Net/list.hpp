//==========
// list.hpp
//==========

// Windows.NET-Implementation of an ordererd list.
// Items can be inserted and removed in constant low time.

// Copyright 2022, Sven Bieg (svenbieg@web.de)
// http://github.com/svenbieg/Clusters.NET


#ifndef _CLUSTERS_NET_LIST_HPP
#define _CLUSTERS_NET_LIST_HPP


//===========
// Namespace
//===========

namespace Clusters {


//=======
// Group
//=======

template <typename _item_t>
ref class _list_group abstract
{
internal:
	// Access
	virtual _item_t^ get_at(int position)=0;
	virtual int get_child_count()=0;
	virtual int get_item_count()=0;
	virtual int get_level()=0;

	// Modification
	virtual bool append(_item_t^ item, bool again)=0;
	virtual bool insert_at(int position, _item_t^ item, bool again)=0;
	virtual void remove_at(int position)=0;
	virtual void set_at(int position, _item_t^ item)=0;
};


//============
// Item-Group
//============

template <typename _item_t, int _group_size>
ref class _list_item_group: public _list_group<_item_t>
{
internal:
	// Con-/Destructors
	_list_item_group(): m_items(_group_size) {}
	_list_item_group(_list_item_group^ group): m_items(%group->m_items) {}
	~_list_item_group() { m_items.Clear(); }

	// Access
	_item_t^ get_at(int position)override
		{
		if(position>=m_items.Count)
			return nullptr;
		return m_items[position];
		}
	inline int get_child_count()override { return m_items.Count; }
	inline int get_item_count()override { return m_items.Count; }
	inline int get_level()override { return 0; }

	// Modification
	bool append(_item_t^ item, bool)override
		{
		if(m_items.Count==_group_size)
			return false;
		m_items.Add(item);
		return true;
		}
	void append_items(System::Collections::Generic::IEnumerable<_item_t^>^ items)
		{
		m_items.InsertRange(m_items.Count, items);
		}
	bool insert_at(int position, _item_t^ item, bool)override
		{
		if(m_items.Count==_group_size)
			return false;
		m_items.Insert(position, item);
		return true;
		}
	void insert_items(int position, System::Collections::Generic::IEnumerable<_item_t^>^ items)
		{
		m_items.InsertRange(position, items);
		}
	void remove_at(int position)override
		{
		m_items.RemoveAt(position);
		}
	System::Collections::Generic::List<_item_t^>^ remove_items(int position, int count)
		{
		auto items=m_items.GetRange(position, count);
		m_items.RemoveRange(position, count);
		return items;
		}
	void set_at(int position, _item_t^ item)override
		{
		m_items[position]=item;
		}

private:
	// Common
	System::Collections::Generic::List<_item_t^> m_items;
};


//==============
// Parent-Group
//==============

template <typename _item_t, int _group_size>
ref class _list_parent_group: public _list_group<_item_t>
{
internal:
	// Using
	using _group_t=_list_group<_item_t>;
	using _item_group_t=_list_item_group<_item_t, _group_size>;
	using _parent_group_t=_list_parent_group<_item_t, _group_size>;

	// Con-Destructors
	_list_parent_group(int level): m_children(_group_size), m_item_count(0), m_level(level) {}
	_list_parent_group(nullptr_t, _group_t^ child):
		m_children(_group_size), m_item_count(child->get_item_count()), m_level(child->get_level()+1)
		{
		m_children.Add(child);
		}
	_list_parent_group(_parent_group_t^ group):
		m_children(%group->m_children), m_item_count(group->m_item_count), m_level(group->m_level)
		{}
	~_list_parent_group() { m_children.Clear(); }

	// Access
	_item_t^ get_at(int position)override
		{
		int group=get_group(&position);
		if(group>=_group_size)
			return nullptr;
		return m_children[group]->get_at(position);
		}
	_group_t^ get_child(int position) { return m_children[position]; }
	inline int get_child_count()override { return m_children.Count; }
	inline int get_item_count()override { return m_item_count; }
	inline int get_level()override { return m_level; }

	// Modification
	bool append(_item_t^ item, bool again)override
		{
		int group=m_children.Count-1;
		if(!again)
			{
			if(m_children[group]->append(item, false))
				{
				m_item_count++;
				return true;
				}
			int empty=get_nearest_space(group);
			if(empty<m_children.Count)
				{
				move_empty_slot(empty, group);
				m_children[group]->append(item, false);
				m_item_count++;
				return true;
				}
			}
		if(!split_child(group))
			return false;
		if(!m_children[group+1]->append(item, true))
			return false;
		m_item_count++;
		return true;
		}
	void append_groups(System::Collections::Generic::IEnumerable<_group_t^>^ groups)
		{
		for each(auto group in groups)
			{
			m_children.Add(group);
			m_item_count+=group->get_item_count();
			}
		}
	bool insert_at(int position, _item_t^ item, bool again)override
		{
		if(position>m_item_count)
			throw gcnew System::IndexOutOfRangeException();
		int pos=position;
		int group=0;
		int insert_count=get_insert_pos(&pos, &group);
		if(!insert_count)
			return false;
		if(!again)
			{
			int at=pos;
			for(int i=0; i<insert_count; i++)
				{
				if(m_children[group+i]->insert_at(at, item, false))
					{
					m_item_count++;
					return true;
					}
				at=0;
				}
			int empty=get_nearest_space(group);
			if(empty<m_children.Count)
				{
				if(insert_count>1&&empty>group)
					group++;
				move_empty_slot(empty, group);
				pos=position;
				insert_count=get_insert_pos(&pos, &group);
				int at=pos;
				for(int i=0; i<insert_count; i++)
					{
					if(m_children[group+i]->insert_at(at, item, false))
						{
						m_item_count++;
						return true;
						}
					at=0;
					}
				}
			}
		if(!split_child(group))
			return false;
		int count=m_children[group]->get_item_count();
		if(pos>=count)
			{
			group++;
			pos-=count;
			}
		m_children[group]->insert_at(pos, item, true);
		m_item_count++;
		return true;
		}
	void insert_groups(int position, System::Collections::Generic::IEnumerable<_group_t^>^ groups)
		{
		for each(auto group in groups)
			{
			m_children.Insert(position++, group);
			m_item_count+=group->get_item_count();
			}
		}
	void move_children(int source, int destination, int count)
		{
		if(m_level>1)
			{
			auto src=(_parent_group_t^)m_children[source];
			auto dst=(_parent_group_t^)m_children[destination];
			if(source>destination)
				{
				auto groups=src->remove_groups(0, count);
				dst->append_groups(groups);
				}
			else
				{
				int src_count=src->get_child_count();
				auto groups=src->remove_groups(src_count-count, count);
				dst->insert_groups(0, groups);
				}
			}
		else
			{
			auto src=(_item_group_t^)m_children[source];
			auto dst=(_item_group_t^)m_children[destination];
			if(source>destination)
				{
				auto items=src->remove_items(0, count);
				dst->append_items(items);
				}
			else
				{
				int src_count=src->get_child_count();
				auto items=src->remove_items(src_count-count, count);
				dst->insert_items(0, items);
				}
			}
		}
	void move_empty_slot(int source, int destination)
		{
		if(source<destination)
			{
			for(int pos=source; pos<destination; pos++)
				move_children(pos+1, pos, 1);
			}
		else
			{
			for(int pos=source; pos>destination; pos--)
				move_children(pos-1, pos, 1);
			}
		}
	void remove_at(int position)override
		{
		if(position>=m_item_count)
			throw gcnew System::IndexOutOfRangeException();
		int group=get_group(&position);
		m_children[group]->remove_at(position);
		m_item_count--;
		combine_children(group);
		}
	System::Collections::Generic::List<_group_t^>^ remove_groups(int position, int count)
		{
		auto groups=m_children.GetRange(position, count);
		for each(auto group in groups)
			{
			m_item_count-=group->get_item_count();
			}
		m_children.RemoveRange(position, count);
		return groups;
		}
	void set_at(int position, _item_t^ item)override
		{
		int group=get_group(&position);
		if(group>=_group_size)
			throw gcnew System::IndexOutOfRangeException();
		m_children[group]->set_at(position, item);
		}

private:
	// Access
	int get_group(int* position)
		{
		for(int i=0; i<m_children.Count; i++)
			{
			int count=m_children[i]->get_item_count();
			if(*position<count)
				return i;
			*position-=count;
			}
		return _group_size;
		}
	int get_insert_pos(int* position, int* group)
		{
		int pos=*position;
		for(int i=0; i<m_children.Count; i++)
			{
			int count=m_children[i]->get_item_count();
			if(pos<=count)
				{
				*group=i;
				*position=pos;
				if(pos==count&&i+1<m_children.Count)
					return 2;
				return 1;
				}
			pos-=count;
			}
		return 0;
		}
	int get_nearest_space(int position)
		{
		int count=m_children.Count;
		int before=position-1;
		int after=position+1;
		while(before>=0||after<count)
			{
			if(before>=0)
				{
				if(m_children[before]->get_child_count()<_group_size)
					return before;
				before--;
				}
			if(after<count)
				{
				if(m_children[after]->get_child_count()<_group_size)
					return after;
				after++;
				}
			}
		return _group_size;
		}

	// Modification
	bool combine_children(int position)
		{
		int current=m_children[position]->get_child_count();
		if(current==0)
			{
			m_children.RemoveAt(position);
			return true;
			}
		if(position>0)
			{
			int before=m_children[position-1]->get_child_count();
			if(current+before<=_group_size)
				{
				move_children(position, position-1, current);
				m_children.RemoveAt(position);
				return true;
				}
			}
		if(position+1<m_children.Count)
			{
			int after=m_children[position+1]->get_child_count();
			if(current+after<=_group_size)
				{
				move_children(position+1, position, after);
				m_children.RemoveAt(position+1);
				return true;
				}
			}
		return false;
		}
	bool split_child(int position)
		{
		if(m_children.Count==_group_size)
			return false;
		_group_t^ group;
		if(m_level>1)
			{
			group=gcnew _parent_group_t(m_level-1);
			}
		else
			{
			group=gcnew _item_group_t();
			}
		m_children.Insert(position+1, group);
		move_children(position, position+1, 1);
		return true;
		}

	// Common
	System::Collections::Generic::List<_group_t^> m_children;
	int m_item_count;
	int m_level;
};


//=========
// Cluster
//=========

template <typename _item_t, int _group_size>
ref class _list_cluster
{
internal:
	// Using
	using _group_t=_list_group<_item_t>;
	using _item_group_t=_list_item_group<_item_t, _group_size>;
	using _parent_group_t=_list_parent_group<_item_t, _group_size>;

	// Access
	_item_t^ get_at(int position)
		{
		if(!m_root)
			return nullptr;
		return m_root->get_at(position);
		}
	int get_count()
		{
		if(!m_root)
			return 0;
		return m_root->get_item_count();
		}

	// Modification
	void append(_item_t^ item)
		{
		if(!m_root)
			m_root=gcnew _item_group_t();
		if(m_root->append(item, false))
			return;
		m_root=gcnew _parent_group_t(nullptr, m_root);
		m_root->append(item, true);
		}
	void clear()
		{
		if(m_root)
			{
			delete m_root;
			m_root=nullptr;
			}
		}
	bool insert_at(int position, _item_t^ item)
		{
		if(!m_root)
			{
			if(position>0)
				return false;
			m_root=gcnew _item_group_t();
			return m_root->append(item, false);
			}
		if(position>m_root->get_item_count())
			return false;
		if(m_root->insert_at(position, item, false))
			return true;
		m_root=gcnew _parent_group_t(nullptr, m_root);
		return m_root->insert_at(position, item, true);
		}
	void remove_at(int position)
		{
		if(!m_root)
			throw gcnew System::IndexOutOfRangeException();
		m_root->remove_at(position);
		update_root();
		}
	void set(_list_cluster^ list)
		{
		clear();
		if(!list->m_root)
			return;
		if(list->m_root->get_level()>0)
			{
			m_root=gcnew _parent_group_t((_parent_group_t^)list->m_root);
			}
		else
			{
			m_root=gcnew _item_group_t((_item_group_t^)list->m_root);
			}
		}
	void set_at(int position, _item_t^ item)
		{
		if(!m_root)
			throw gcnew System::IndexOutOfRangeException();
		m_root->set_at(position, item);
		}

	// Common
	_group_t^ m_root;

protected:
	// Con-/Destructors
	_list_cluster() {}

private:
	// Common
	void update_root()
		{
		if(m_root->get_level()==0)
			{
			if(m_root->get_child_count()==0)
				{
				delete m_root;
				m_root=nullptr;
				}
			return;
			}
		if(m_root->get_child_count()>1)
			return;
		_parent_group_t^ root=(_parent_group_t^)m_root;
		m_root=root->get_child(0);
		delete root;
		}
};


//==========
// Iterator
//==========

template <class _item_t>
ref class _list_it_struct
{
internal:
	_list_it_struct(): position(0) {}
	_list_group<_item_t>^ group;
	int position;
};

template <typename _item_t, int _group_size>
ref class _list_iterator
{
internal:
	// Using
	using _group_t=_list_group<_item_t>;
	using _it_struct_t=_list_it_struct<_item_t>;
	using _item_group_t=_list_item_group<_item_t, _group_size>;
	using _list_t=_list_cluster<_item_t, _group_size>;
	using _parent_group_t=_list_parent_group<_item_t, _group_size>;

	// Con-/Destructors
	_list_iterator(_list_t^ list, int position):
		m_its(list->m_root->get_level()+1), m_list(list)
		{
		int level_count=list->m_root->get_level()+1;
		for(int i=0; i<level_count; i++)
			{
			auto it=gcnew _it_struct_t();
			m_its.Add(it);
			}
		set_position(position);
		}
	~_list_iterator() { m_its.Clear(); }

	// Access
	_item_t^ get_current() { return m_current; }
	int get_position()
		{
		int level_count=m_its.Count;
		int pos=0;
		for(int i=0; i<level_count-1; i++)
			{
			auto group=(_parent_group_t^)(m_its[i]->group);
			int group_pos=m_its[i]->position;
			for(int j=0; j<group_pos; j++)
				pos+=group->get_child(j)->get_item_count();
			}
		pos+=m_its[level_count-1]->position;
		return pos;
		}
	inline bool has_current() { return m_current!=nullptr; }

	// Navigation
	bool move_next()
		{
		int level_count=m_its.Count;
		auto it=m_its[level_count-1];
		auto item_group=(_item_group_t^)it->group;
		int count=item_group->get_child_count();
		if(it->position+1<count)
			{
			it->position++;
			m_current=item_group->get_at(it->position);
			return true;
			}
		for(int i=level_count-1; i>0; i--)
			{
			it=m_its[i-1];
			auto parent_group=(_parent_group_t^)it->group;
			count=parent_group->get_child_count();
			if(it->position+1>=count)
				continue;
			it->position++;
			_group_t^ group=it->group;
			for(; i<level_count; i++)
				{
				parent_group=(_parent_group_t^)group;
				group=parent_group->get_child(it->position);
				it=m_its[i];
				it->group=group;
				it->position=0;
				}
			item_group=(_item_group_t^)group;
			m_current=item_group->get_at(0);
			return true;
			}
		m_current=nullptr;
		return false;
		}
	bool move_previous()
		{
		int level_count=m_its.Count;
		auto it=m_its[level_count-1];
		auto item_group=(_item_group_t^)it->group;
		if(it->position>0)
			{
			it->position--;
			m_current=item_group->get_at(it->position);
			return true;
			}
		for(int i=level_count-1; i>0; i--)
			{
			it=m_its[i-1];
			auto parent_group=(_parent_group_t^)it->group;
			if(it->position==0)
				continue;
			it->position--;
			_group_t^ group=it->group;
			int pos=0;
			for(; i<level_count; i++)
				{
				parent_group=(_parent_group_t^)group;
				group=parent_group->get_child(it->position);
				pos=group->get_child_count()-1;
				it=m_its[i];
				it->group=group;
				it->position=pos;
				}
			item_group=(_item_group_t^)group;
			m_current=item_group->get_at(pos);
			return true;
			}
		m_current=nullptr;
		return false;
		}
	bool set_position(int position)
		{
		m_current=nullptr;
		_group_t^ group=m_list->m_root;
		if(!group)
			return false;
		int level_count=m_its.Count;
		if(position==-1)
			{
			int count=m_list->get_count();
			position=count-1;
			}
		int pos=get_position_internal(group, &position);
		m_its[0]->group=group;
		m_its[0]->position=pos;
		for(int i=0; i<level_count-1; i++)
			{
			auto parent_group=(_parent_group_t^)(m_its[i]->group);
			group=parent_group->get_child(pos);
			pos=get_position_internal(group, &position);
			m_its[i+1]->group=group;
			m_its[i+1]->position=pos;
			}
		if(pos<group->get_child_count())
			{
			auto item_group=(_item_group_t^)group;
			m_current=item_group->get_at(pos);
			return true;
			}
		return false;
		}

	// Modification
	bool remove_current()
		{
		if(m_current==nullptr)
			throw gcnew System::IndexOutOfRangeException();
		int pos=get_position();
		m_list->remove_at(pos);
		return set_position(pos);
		}
	void set_current(_item_t^ item)
		{
		if(m_current==nullptr)
			throw gcnew System::IndexOutOfRangeException();
		m_current->set_item(item);
		}

private:
	// Common
	int get_position_internal(_group_t^ group, int* pos)
		{
		int count=group->get_child_count();
		int level=group->get_level();
		if(level==0)
			{
			int i=*pos;
			*pos=0;
			return i;
			}
		auto parent_group=(_parent_group_t^)group;
		int item_count=0;
		for(int i=0; i<count; i++)
			{
			_group_t^ child=parent_group->get_child(i);
			item_count=child->get_item_count();
			if(*pos<item_count)
				return i;
			*pos-=item_count;
			}
		return _group_size;
		}
	_item_t^ m_current;
	System::Collections::Generic::List<_it_struct_t^> m_its;
	_list_t^ m_list;
};


//======
// List
//======

template <typename _item_t, int _group_size=10>
ref class _list: public _list_cluster<_item_t, _group_size>
{
internal:
	// Using
	using _base_t=_list_cluster<_item_t, _group_size>;
	using _it_t=_list_iterator<_item_t, _group_size>;

	// Con-/Destructors
	_list() {}

	// Navigation
	inline _it_t^ at(int position) { return gcnew _it_t(this, position); }
	inline _it_t^ first() { return gcnew _it_t(this, 0); }
	inline _it_t^ last() { return gcnew _it_t(this, -1); }
};

} // namespace

#endif // _CLUSTERS_NET_LIST_HPP
