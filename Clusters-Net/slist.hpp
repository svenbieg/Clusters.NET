//===========
// slist.hpp
//===========

// Windows.NET-Implementation of a sorted list.
// Items and can be inserted, removed and looked-up in constant low time.

// Copyright 2022, Sven Bieg (svenbieg@web.de)
// http://github.com/svenbieg/Clusters.NET


#ifndef _CLUSTERS_NET_SLIST_HPP
#define _CLUSTERS_NET_SLIST_HPP


//===========
// Namespace
//===========

namespace Clusters {


//======
// Item
//======

template <typename _id_t, typename _item_t>
ref class _slist_item
{
internal:
	_slist_item(_slist_item^ item): m_id(item->m_id), m_item(item->m_item) {}
	_slist_item(_id_t id, _item_t item): m_id(id), m_item(item) {}
	_id_t get_id() { return m_id; }
	_item_t get_item() { return m_item; }
	void set(_item_t item) { m_item=item; }

private:
	_id_t m_id;
	_item_t m_item;
};


//=======
// Group
//=======

template <typename _id_t, typename _item_t>
ref class _slist_group abstract
{
internal:
	// Using 
	using _slist_item_t=_slist_item<_id_t, _item_t>;

	// Access
	virtual bool contains(_id_t id)=0;
	virtual int find(_id_t id)=0;
	virtual _slist_item_t^ get(_id_t id)=0;
	virtual _slist_item_t^ get_at(int position)=0;
	virtual int get_child_count()=0;
	virtual _slist_item_t^ get_first()=0;
	virtual int get_item_count()=0;
	virtual _slist_item_t^ get_last()=0;
	virtual int get_level()=0;

	// Modification
	virtual bool add(_id_t id, _item_t item, bool again, bool* exists)=0;
	virtual bool remove(_id_t id)=0;
	virtual bool remove_at(int position)=0;
	virtual bool set(_id_t id, _item_t item, bool again, bool* exists)=0;
};


//============
// Item-Group
//============

template <typename _id_t, typename _item_t, int _group_size>
ref class _slist_item_group: public _slist_group<_id_t, _item_t>
{
internal:
	// Using
	using _slist_item_t=_slist_item<_id_t, _item_t>;

	// Con-/Destructors
	_slist_item_group(): m_items(_group_size) {}
	_slist_item_group(_slist_item_group^ group): m_items(%group->m_items) {}
	~_slist_item_group() { m_items.Clear(); }

	// Access
	inline bool contains(_id_t id)override { return get_item_pos(id)>=0; }
	inline int find(_id_t id)override { return get_item_pos(id); }
	_slist_item_t^ get(_id_t id)override
		{
		int pos=get_item_pos(id);
		if(pos<0)
			return nullptr;
		return m_items[pos];
		}
	_slist_item_t^ get_at(int position)override
		{
		if(position>=m_items.Count)
			return nullptr;
		return m_items[position];
		}
	inline int get_child_count()override { return m_items.Count; }
	_slist_item_t^ get_first()override
		{
		if(m_items.Count==0)
			return nullptr;
		return m_items[0];
		}
	inline int get_item_count()override { return m_items.Count; }
	_slist_item_t^ get_last()override
		{
		int count=m_items.Count;
		if(count==0)
			return nullptr;
		return m_items[count-1];
		}
	inline int get_level()override { return 0; }

	// Modification
	bool add(_id_t id, _item_t item, bool again, bool* exists)override
		{
		int pos=get_insert_pos(id, exists);
		if(*exists)
			return false;
		return add_internal(id, item, pos);
		}
	void append_items(System::Collections::Generic::IEnumerable<_slist_item_t^>^ items)
		{
		int count=m_items.Count;
		m_items.InsertRange(count, items);
		}
	void insert_items(int position, System::Collections::Generic::IEnumerable<_slist_item_t^>^ items)
		{
		m_items.InsertRange(position, items);
		}
	bool remove(_id_t id)override
		{
		int pos=get_item_pos(id);
		if(pos<0)
			return false;
		return remove_at(pos);
		}
	bool remove_at(int position)override
		{
		if(position>=m_items.Count)
			return false;
		m_items.RemoveAt(position);
		return true;
		}
	System::Collections::Generic::List<_slist_item_t^>^ remove_items(int position, int count)
		{
		auto items=m_items.GetRange(position, count);
		m_items.RemoveRange(position, count);
		return items;
		}
	bool set(_id_t id, _item_t item, bool again, bool *exists)override
		{
		int pos=get_insert_pos(id, exists);
		if(*exists)
			{
			m_items[pos]->set(item);
			return true;
			}
		return add_internal(id, item, pos);
		}

private:
	// Access
	int get_insert_pos(_id_t id, bool* exists)
		{
		int start=0;
		int end=m_items.Count;
		while(start<end)
			{
			int pos=start+(end-start)/2;
			_id_t item_id=m_items[pos]->get_id();
			if(item_id>id)
				{
				end=pos;
				continue;
				}
			if(item_id<id)
				{
				start=(int)(pos+1);
				continue;
				}
			*exists=true;
			return pos;
			}
		return start;
		}
	int get_item_pos(_id_t id)
		{
		int start=0;
		int end=m_items.Count;
		int pos=0;
		while(start<end)
			{
			pos=start+(end-start)/2;
			_id_t item_id=m_items[pos]->get_id();
			if(item_id>id)
				{
				end=pos;
				continue;
				}
			if(item_id<id)
				{
				start=(int)(pos+1);
				continue;
				}
			return pos;
			}
		return -pos-1;
		}

	// Modification
	bool add_internal(_id_t id, _item_t item, int pos)
		{
		int count=m_items.Count;
		if(count==_group_size)
			return false;
		auto slist_item=gcnew _slist_item_t(id, item);
		m_items.Insert(pos, slist_item);
		return true;
		}

	// Items
	System::Collections::Generic::List<_slist_item_t^> m_items;
};


//==============
// Parent-Group
//==============

template <typename _id_t, typename _item_t, int _group_size>
ref class _slist_parent_group: public _slist_group<_id_t, _item_t>
{
internal:
	// Using
	using _group_t=_slist_group<_id_t, _item_t>;
	using _item_group_t=_slist_item_group<_id_t, _item_t, _group_size>;
	using _parent_group_t=_slist_parent_group<_id_t, _item_t, _group_size>;
	using _slist_item_t=_slist_item<_id_t, _item_t>;

	// Con-Destructors
	_slist_parent_group(int level):
		m_children(_group_size), m_first(nullptr), m_item_count(0), m_last(nullptr), m_level(level) {}
	_slist_parent_group(nullptr_t, _group_t^ child):
		m_children(_group_size), m_first(child->get_first()), m_item_count(child->get_item_count()),
		m_last(child->get_last()), m_level(child->get_level()+1)
		{
		m_children.Add(child);
		}
	_slist_parent_group(_parent_group_t^ group):
		m_children(%group->m_children), m_item_count(group->m_item_count), m_level(group->m_level)
		{
		int count=m_children.Count;
		if(!count)
			return;
		m_first=m_children[0]->get_first();
		m_last=m_children[count-1]->get_last();
		}
	~_slist_parent_group() { m_children.Clear(); }

	// Access
	inline bool contains(_id_t id)override { return get_item_pos(id)>=0; }
	inline int find(_id_t id)override { return get_item_pos(id); }
	_slist_item_t^ get(_id_t id)override
		{
		int pos=get_item_pos(id);
		if(pos<0)
			return nullptr;
		return m_children[pos]->get(id);
		}
	_slist_item_t^ get_at(int position)override
		{
		int group=get_group(&position);
		if(group>=_group_size)
			return nullptr;
		return m_children[group]->get_at(position);
		}
	_group_t^ get_child(int position) { return m_children[position]; }
	inline int get_child_count()override { return m_children.Count; }
	inline _slist_item_t^ get_first()override { return m_first; }
	inline int get_item_count()override { return m_item_count; }
	inline _slist_item_t^ get_last()override { return m_last; }
	inline int get_level()override { return m_level; }

	// Modification
	bool add(_id_t id, _item_t item, bool again, bool* exists)override
		{
		if(add_internal(id, item, again, exists))
			{
			m_item_count++;
			update_bounds();
			return true;
			}
		return false;
		}
	void append_groups(System::Collections::Generic::IEnumerable<_group_t^>^ groups)
		{
		for each(auto group in groups)
			{
			m_children.Add(group);
			m_item_count+=group->get_item_count();
			}
		update_bounds();
		}
	void insert_groups(int position, System::Collections::Generic::IEnumerable<_group_t^>^ groups)
		{
		for each(auto group in groups)
			{
			m_children.Insert(position++, group);
			m_item_count+=group->get_item_count();
			}
		update_bounds();
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
	void move_empty_space(int source, int destination)
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
	bool remove(_id_t id)override
		{
		int pos=get_item_pos(id);
		if(pos<0)
			return false;
		if(!m_children[pos]->remove(id))
			return false;
		m_item_count--;
		combine_children(pos);
		update_bounds();
		return true;
		}
	bool remove_at(int position)override
		{
		if(position>=m_item_count)
			return false;
		int group=get_group(&position);
		m_children[group]->remove_at(position);
		m_item_count--;
		combine_children(group);
		update_bounds();
		return true;
		}
	System::Collections::Generic::List<_group_t^>^ remove_groups(int position, int count)
		{
		auto groups=m_children.GetRange(position, count);
		for each(auto group in groups)
			{
			m_item_count-=group->get_item_count();
			}
		m_children.RemoveRange(position, count);
		update_bounds();
		return groups;
		}
	bool set(_id_t id, _item_t item, bool again, bool* exists)override
		{
		if(set_internal(id, item, again, exists))
			{
			if(!(*exists))
				{
				m_item_count++;
				update_bounds();
				}
			return true;
			}
		return false;
		}

private:
	// Access
	int get_group(int* position)
		{
		int count=m_children.Count;
		for(int pos=0; pos<count; pos++)
			{
			int item_count=m_children[pos]->get_item_count();
			if(*position<count)
				return pos;
			*position-=item_count;
			}
		return _group_size;
		}
	int get_item_pos(_id_t id)
		{
		int start=0;
		int end=m_children.Count;
		int pos=0;
		while(start<end)
			{
			pos=start+(end-start)/2;
			_slist_item_t^ first=m_children[pos]->get_first();
			if(first->get_id()>id)
				{
				end=pos;
				continue;
				}
			_slist_item_t^ last=m_children[pos]->get_last();
			if(last->get_id()<id)
				{
				start=(int)(pos+1);
				continue;
				}
			return pos;
			}
		return -pos-1;
		}
	int get_insert_pos(_id_t id, int* group, bool* exists)
		{
		int count=m_children.Count;
		if(!count)
			return 0;
		int start=0;
		int end=count;
		while(start<end)
			{
			int pos=start+(end-start)/2;
			_slist_item_t^ first=m_children[pos]->get_first();
			_slist_item_t^ last=m_children[pos]->get_last();
			_id_t first_id=first->get_id();
			_id_t last_id=last->get_id();
			if(first_id==id||last_id==id)
				{
				*exists=true;
				*group=pos;
				return 1;
				}
			if(first_id>id)
				{
				end=pos;
				continue;
				}
			if(last_id<id)
				{
				start=pos+1;
				continue;
				}
			start=pos;
			break;
			}
		if(start>count-1)
			start=count-1;
		*group=start;
		if(start>0)
			{
			_slist_item_t^ first=m_children[start]->get_first();
			if(first->get_id()>=id)
				{
				*group=start-1;
				return 2;
				}
			}
		if(start+1<count)
			{
			_slist_item_t^ last=m_children[start]->get_last();
			if(last->get_id()<=id)
				return 2;
			}
		return 1;
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
	bool add_internal(_id_t id, _item_t item, bool again, bool* exists)
		{
		int group=0;
		int count=get_insert_pos(id, &group, exists);
		if(*exists)
			return false;
		if(!again)
			{
			for(int u=0; u<count; u++)
				{
				if(m_children[group+u]->add(id, item, false, exists))
					return true;
				if(*exists)
					return false;
				}
			if(shift_children(group, count))
				{
				count=get_insert_pos(id, &group, exists);
				for(int u=0; u<count; u++)
					{
					if(m_children[group+u]->add(id, item, false, exists))
						return true;
					}
				}
			}
		if(!split_child(group))
			return false;
		count=get_insert_pos(id, &group, exists);
		for(int u=0; u<count; u++)
			{
			if(m_children[group+u]->add(id, item, true, exists))
				return true;
			}
		return false;
		}
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
	bool set_internal(_id_t id, _item_t item, bool again, bool* exists)
		{
		int pos=get_item_pos(id);
		if(pos>=0)
			{
			if(m_children[pos]->set(id, item, again, exists))
				return true;
			}
		return add_internal(id, item, again, exists);
		}
	bool shift_children(int group, int count)
		{
		int child_count=m_children.Count;
		int empty=get_nearest_space(group);
		if(empty>=child_count)
			return false;
		if(count>1&&empty>group)
			group++;
		move_empty_space(empty, group);
		return true;
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
	void update_bounds()
		{
		int child_count=m_children.Count;
		if(!child_count)
			return;
		m_first=m_children[0]->get_first();
		m_last=m_children[child_count-1]->get_last();
		}
	
	// Common
	System::Collections::Generic::List<_group_t^> m_children;
	_slist_item_t^ m_first;
	int m_item_count;
	_slist_item_t^ m_last;
	int m_level;
};


//=========
// Cluster
//=========

template <typename _id_t, typename _item_t, int _group_size>
ref class _slist_cluster
{
internal:
	// Using
	using _group_t=_slist_group<_id_t, _item_t>;
	using _item_group_t=_slist_item_group<_id_t, _item_t, _group_size>;
	using _parent_group_t=_slist_parent_group<_id_t, _item_t, _group_size>;

	// Access
	bool contains(_id_t id)
		{
		if(!m_root)
			return false;
		return m_root->contains(id);
		}
	int get_count()
		{
		if(!m_root)
			return 0;
		return m_root->get_item_count();
		}

	// Modification
	bool add(_id_t id, _item_t item)
		{
		if(m_root==nullptr)
			m_root=gcnew _item_group_t();
		bool exists=false;
		if(m_root->add(id, item, false, &exists))
			return true;
		if(exists)
			return false;
		m_root=gcnew _parent_group_t(nullptr, m_root);
		return m_root->add(id, item, true, &exists);
		}
	void clear()
		{
		if(m_root)
			{
			delete m_root;
			m_root=nullptr;
			}
		}
	bool remove(_id_t id)
		{
		if(!m_root)
			return false;
		if(m_root->remove(id))
			{
			update_root();
			return true;
			}
		return false;
		}
	bool remove_at(int position)
		{
		if(!m_root)
			return false;
		if(m_root->remove_at(position))
			{
			update_root();
			return true;
			}
		return false;
		}
	void set(_slist_cluster^ slist)
		{
		clear();
		if(!slist->m_root)
			return;
		if(slist->m_root->get_level()>0)
			{
			m_root=gcnew _parent_group_t((_parent_group_t^)slist->m_root);
			}
		else
			{
			m_root=gcnew _item_group_t((_item_group_t^)slist->m_root);
			}
		}
	void set(_id_t id, _item_t item)
		{
		if(m_root==nullptr)
			m_root=gcnew _item_group_t();
		bool exists=false;
		if(m_root->set(id, item, false, &exists))
			return;
		m_root=gcnew _parent_group_t(nullptr, m_root);
		m_root->set(id, item, true, &exists);
		}

	// Common
	_group_t^ m_root;

protected:
	// Con-/Destructors
	_slist_cluster() {}

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

template <typename _id_t, typename _item_t>
ref class _slist_it_struct
{
internal:
	_slist_it_struct(): position(0) {}
	_slist_group<_id_t, _item_t>^ group;
	int position;
};

template <typename _id_t, typename _item_t, int _group_size>
ref class _slist_iterator
{
internal:
	// Using
	using _group_t=_slist_group<_id_t, _item_t>;
	using _it_struct_t=_slist_it_struct<_id_t, _item_t>;
	using _it_t=_slist_iterator<_id_t, _item_t, _group_size>;
	using _item_group_t=_slist_item_group<_id_t, _item_t, _group_size>;
	using _parent_group_t=_slist_parent_group<_id_t, _item_t, _group_size>;
	using _slist_item_t=_slist_item<_id_t, _item_t>;
	using _slist_t=_slist_cluster<_id_t, _item_t, _group_size>;

	// Con-/Destructors
	_slist_iterator(_slist_t^ slist):
		m_its(slist->m_root->get_level()+1), m_slist(slist)
		{
		int level_count=slist->m_root->get_level()+1;
		for(int i=0; i<level_count; i++)
			{
			auto it=gcnew _it_struct_t();
			m_its.Add(it);
			}
		}
	_slist_iterator(_slist_t^ slist, int position): _it_t(slist) { set_position(position); }
	_slist_iterator(_slist_t^ slist, int, _id_t id): _it_t(slist) { find(id); }
	~_slist_iterator() { m_its.Clear(); }

	// Access
	_id_t get_current_id()
		{
		if(m_current==nullptr)
			throw gcnew System::IndexOutOfRangeException();
		return m_current->get_id();
		}
	_item_t get_current_item()
		{
		if(m_current==nullptr)
			throw gcnew System::IndexOutOfRangeException();
		return m_current->get_item();
		}
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
	bool find(_id_t id)
		{
		m_current=nullptr;
		bool found=true;
		_group_t^ group=m_slist->m_root;
		if(group==nullptr)
			return false;
		int level_count=m_its.Count;
		for(int i=0; i<level_count-1; i++)
			{
			auto parent_group=(_parent_group_t^)group;
			int pos=parent_group->find(id);
			if(pos<0)
				{
				found=false;
				pos++;
				pos*=-1;
				}
			m_its[i]->group=group;
			m_its[i]->position=pos;
			group=parent_group->get_child(pos);
			}
		auto item_group=(_item_group_t^)group;
		int pos=item_group->find(id);
		if(pos<0)
			{
			found=false;
			pos++;
			pos*=-1;
			}
		m_its[level_count-1]->group=group;
		m_its[level_count-1]->position=pos;
		m_current=item_group->get_at(pos);
		return found;
		}
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
		_group_t^ group=m_slist->m_root;
		if(!group)
			return false;
		int level_count=m_its.Count;
		if(position==-1)
			{
			int count=m_slist->get_count();
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
		m_slist->remove_at(pos);
		return set_position(pos);
		}
	void set_current_item(_item_t item)
		{
		if(m_current==nullptr)
			throw gcnew System::IndexOutOfRangeException();
		m_current->set(item);
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
	_slist_item_t^ m_current;
	System::Collections::Generic::List<_it_struct_t^> m_its;
	_slist_t^ m_slist;
};


//=======
// SList
//=======

template <typename _id_t, typename _item_t, int _group_size=10>
ref class _slist: public _slist_cluster<_id_t, _item_t, _group_size>
{
internal:
	// Using
	using _base_t=_slist_cluster<_id_t, _item_t, _group_size>;
	using _group_t=_slist_group<_id_t, _item_t>;
	using _it_t=_slist_iterator<_id_t, _item_t, _group_size>;
	using _item_group_t=_slist_item_group<_id_t, _item_t, _group_size>;
	using _parent_group_t=_slist_parent_group<_id_t, _item_t, _group_size>;
	using _slist_item_t=_slist_item<_id_t, _item_t>;

	// Con-/Destructors
	_slist() {}

	// Access
	_item_t get(_id_t id)
		{
		if(this->m_root==nullptr)
			return nullptr;
		_slist_item_t^ item=this->m_root->get(id);
		if(item==nullptr)
			return nullptr;
		return item->get_item();
		}

	// Navigation
	inline _it_t^ at(int position) { return gcnew _it_t(this, position); }
	inline _it_t^ find(_id_t id) { return gcnew _it_t(this, 0, id); }
	inline _it_t^ first() { return gcnew _it_t(this, 0); }
	inline _it_t^ last() { return gcnew _it_t(this, -1); }
};

} // namespace

#endif // _CLUSTERS_NET_SLIST_HPP
