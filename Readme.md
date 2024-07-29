<h1>Clusters.NET</h1>

<p>
This is the .NET-implementation of
<a href="http://github.com/svenbieg/clusters">Clusters</a>
used for ordering and sorting.<br />
<img src="https://github.com/svenbieg/Clusters.NET/assets/12587394/4ea7ffb9-b870-47e7-a20f-899c64e9b1ec" /><br />
It has some advatages compared to the default List and Dictionary.<br />
The List is not an array and multiple times faster.<br />
My Index and my Map can be looked up by near-by values.<br />
<br />
There is a package available at
<a href="https://www.nuget.org/packages/Clusters">nuget.org</a>
<br />
<br />

<h2>Principle</h2>
<br />

<table>
	<tr>
		<td>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<img src="https://user-images.githubusercontent.com/12587394/47256722-d3dc4580-d484-11e8-8393-b0e7c026be5e.png" /></td>
		<td>The entries are stored in groups.</td>
	</tr><tr><td></td></tr><tr>
		<td>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<img src="https://user-images.githubusercontent.com/12587394/47256729-e48cbb80-d484-11e8-833e-846bb4a70b0c.png" /></td>
		<td>The size of the groups is limited and 10 by default.</td>
	</tr><tr><td></td></tr><tr>
		<td>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<img src="https://user-images.githubusercontent.com/12587394/47256737-f4a49b00-d484-11e8-9171-a40ef63c3ff1.png" /></td>
		<td>If the group is full a parent-group is created.</td>
	</tr><tr><td></td></tr><tr>
		<td>&nbsp;&nbsp;&nbsp;&nbsp;<img src="https://user-images.githubusercontent.com/12587394/47256739-ff5f3000-d484-11e8-9445-4443f52e228a.png" /></td>
		<td>The first and the last entry can be moved to the neighbour-group.</td>
	</tr><tr><td></td></tr><tr>
		<td><img src="https://user-images.githubusercontent.com/12587394/47256742-09812e80-d485-11e8-8ca6-06a011e88120.png" /></td>
		<td>The entries are moved between the groups, so all groups get as full as possible.</td>
	</tr><tr><td></td></tr><tr>
		<td><img src="https://user-images.githubusercontent.com/12587394/47256745-1736b400-d485-11e8-9785-e0479250b51d.png" /></td>
		<td>The number of groups is limited too, another parent-group is created.</td>
	</tr><tr><td></td></tr><tr>
		<td><img src="https://user-images.githubusercontent.com/12587394/47256748-21f14900-d485-11e8-9506-db75fa50c9bd.png" /></td>
		<td>If an entry needs to be inserted in a full group, a whole sub-tree can be moved.</td>
	</tr>
</table><br />

<p>
You can find detailed information in the
<a href="https://github.com/svenbieg/Clusters.NET/wiki/Home">Wiki</a>.
</p>
